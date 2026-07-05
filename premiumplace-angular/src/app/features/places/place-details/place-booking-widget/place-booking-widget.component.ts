import {
  ChangeDetectionStrategy,
  Component,
  computed,
  DestroyRef,
  effect,
  inject,
  input,
  output,
  signal,
} from '@angular/core';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';

import { BookingsService } from '../../../../core/bookings/bookings.service';
import {
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';

import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatNativeDateModule } from '@angular/material/core';
import { DateRange, MatDatepickerModule } from '@angular/material/datepicker';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';

@Component({
  selector: 'app-place-booking-widget',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatCardModule,
    MatDatepickerModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatNativeDateModule,
  ],
  templateUrl: './place-booking-widget.component.html',
  styleUrls: ['./place-booking-widget.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PlaceBookingWidgetComponent {
  // --- Inputs ---
  placeId = input.required<number>();
  rate = input.required<number>();
  reviewAvg = input.required<number>();
  reviewCount = input.required<number>();
  currencySymbol = input<string>('€');

  // --- Outputs (signal-based) ---
  readonly dateRangeChange = output<{ start: Date | null; end: Date | null }>();
  readonly bookClicked = output<{ placeId: number; start: Date; end: Date }>();

  private readonly bookings = inject(BookingsService);
  private readonly destroyRef = inject(DestroyRef);

  /** Booked (blocked) day ranges as [from, toExcl) in local-day epoch ms. */
  private readonly blocked = signal<{ from: number; toExcl: number }[]>([]);

  // --- Reactive form ---
  readonly form = new FormGroup({
    start: new FormControl<Date | null>(null, Validators.required),
    end: new FormControl<Date | null>(null, Validators.required),
  });

  // --- Signals ---
  private readonly formValue = toSignal(this.form.valueChanges);

  readonly range = computed(() => {
    const v = this.formValue();
    return {
      start: v?.start ?? null,
      end: v?.end ?? null,
    };
  });

  readonly errorText = signal<string | null>(null);

  // --- Computed ---

  /** DateRange fed to the inline mat-calendar. */
  readonly selectedDateRange = computed(
    () => new DateRange<Date>(this.range().start, this.range().end),
  );

  /** Number of nights between start and end (0 when invalid). */
  readonly nights = computed(() => {
    const { start, end } = this.range();
    if (!start) return 0;

    // single-day selection = 1 night
    if (end && end.getTime() === start.getTime()) {
      return 1;
    }

    if (!end) return 0;

    const ms = end.getTime() - start.getTime();
    const n = Math.floor(ms / (1000 * 60 * 60 * 24));
    return n > 0 ? n : 0;
  });

  /** Whether the Book button should be enabled. */
  readonly canBook = computed(() => this.nights() > 0);

  /** Total price for the selected stay. */
  readonly totalPrice = computed(() => this.nights() * this.rate());

  /** Earliest selectable date (today). */
  readonly minDate = new Date();

  /** Disables booked days in the calendar (greyed out + not selectable). */
  readonly dateFilter = computed(() => {
    const ranges = this.blocked();
    return (d: Date | null): boolean => d === null || !isBlocked(d, ranges);
  });

  /** Marks booked days with a class for distinct styling. */
  readonly dateClass = computed(() => {
    const ranges = this.blocked();
    return (d: Date): string => (isBlocked(d, ranges) ? 'pp-blocked' : '');
  });

  // --- Private ---
  private _selectingEnd = false;

  ngOnInit(): void {
    const today = new Date();
    this.bookings
      .availability(this.placeId(), isoOf(today), isoOf(addDays(today, 365)))
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (a) => this.blocked.set(
          (a.blockedRanges ?? []).map(r => ({ from: parseIsoMs(r.from), toExcl: parseIsoMs(r.to) })),
        ),
        error: () => { /* availability is best-effort; backend still guards overlaps */ },
      });
  }

  constructor() {
    effect(() => {
      const v = this.formValue();
      if (v === undefined) return;

      const start = v.start ?? null;
      const end = v.end ?? null;

      this.dateRangeChange.emit({ start, end });
      this.errorText.set(null);

      // Keep calendar click state in sync with form values
      if (start && end) this._selectingEnd = false;
      else if (!start) this._selectingEnd = false;
    }, { allowSignalWrites: true });
  }

  // --- Handlers ---

  /**
   * Two-click calendar selection: first click sets start, second sets end.
   * If the second date is before the first, the pair is swapped automatically.
   */
  onCalendarDateClicked(date: Date | null): void {
    if (!date) return;
    if (isBlocked(date, this.blocked())) return;

    const currentStart = this.form.controls.start.value;

    if (!this._selectingEnd || !currentStart) {
      this.form.controls.start.setValue(date);
      this.form.controls.end.setValue(null);
      this._selectingEnd = true;
      return;
    }

    if (date > currentStart) {
      this.form.controls.end.setValue(date);
    } else {
      // swap
      this.form.controls.end.setValue(currentStart);
      this.form.controls.start.setValue(date);
    }
    this._selectingEnd = false;
  }

  /** Emits bookClicked when the selected range is valid. */
  onBook(): void {
    if (!this.canBook()) return;

    const start = this.form.controls.start.value!;
    let end = this.form.controls.end.value!;

    if (start && end && start.getTime() === end.getTime()) {
      end = new Date(start);
      end.setDate(end.getDate() + 1);
    }

    if (!start || !end) return;

    // TODO: check auth before booking
    // TODO: integrate availability endpoint + disable blocked dates using dateFilter
    // TODO: handle API errors

    this.bookClicked.emit({ placeId: this.placeId(), start, end });
  }
}

/** Start-of-local-day epoch ms for a date. */
function startOfDayMs(d: Date): number {
  return new Date(d.getFullYear(), d.getMonth(), d.getDate()).getTime();
}

/** True when the date falls inside any [from, toExcl) booked range. */
function isBlocked(d: Date, ranges: { from: number; toExcl: number }[]): boolean {
  const t = startOfDayMs(d);
  return ranges.some(r => t >= r.from && t < r.toExcl);
}

/** Parses a yyyy-MM-dd string to local-day epoch ms. */
function parseIsoMs(iso: string): number {
  const [y, m, day] = iso.split('-').map(Number);
  return new Date(y, m - 1, day).getTime();
}

/** Formats a Date as a local yyyy-MM-dd string. */
function isoOf(d: Date): string {
  const y = d.getFullYear();
  const m = String(d.getMonth() + 1).padStart(2, '0');
  const day = String(d.getDate()).padStart(2, '0');
  return `${y}-${m}-${day}`;
}

function addDays(d: Date, n: number): Date {
  const x = new Date(d);
  x.setDate(x.getDate() + n);
  return x;
}
