import { CommonModule } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';

import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';

import { PlacesService } from '../../../core/places/places.service';
import type { PlaceFormRequest, PlacePreview, PlaceFeatures, PlaceOptions } from '../../../core/places/places.models';

type FeatureKey = keyof PlaceFeatures;

const FEATURE_OPTIONS: Array<{ key: FeatureKey; label: string }> = [
  { key: 'internet', label: 'Wi-Fi' },
  { key: 'airConditioned', label: 'Air conditioning' },
  { key: 'petsAllowed', label: 'Pets allowed' },
  { key: 'parking', label: 'Parking' },
  { key: 'entertainment', label: 'Entertainment' },
  { key: 'kitchen', label: 'Kitchen' },
  { key: 'refrigerator', label: 'Refrigerator' },
  { key: 'washer', label: 'Washer' },
  { key: 'dryer', label: 'Dryer' },
  { key: 'selfCheckIn', label: 'Self check-in' },
];

@Component({
  selector: 'app-admin-page',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatButtonModule,
    MatCheckboxModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatSelectModule,
  ],
  templateUrl: './admin-page.component.html',
  styleUrl: './admin-page.component.scss',
})
export class AdminPageComponent {
  private readonly fb = inject(FormBuilder);
  private readonly placesService = inject(PlacesService);

  readonly places = this.placesService.places;
  readonly loading = this.placesService.loadingList;
  readonly saving = signal(false);
  readonly error = signal<string | null>(null);
  readonly selectedPlace = signal<PlacePreview | null>(null);
  readonly placePendingDelete = signal<PlacePreview | null>(null);
  readonly options = signal<PlaceOptions>({ cities: [], amenities: [] });
  readonly featureOptions = FEATURE_OPTIONS;

  readonly formTitle = computed(() =>
    this.selectedPlace() ? `Edit ${this.selectedPlace()!.name}` : 'Create place'
  );

  readonly form = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(200)]],
    details: ['', [Validators.maxLength(1000)]],
    guestCapacity: [1, [Validators.required, Validators.min(1), Validators.max(30)]],
    rate: [0, [Validators.required, Validators.min(0), Validators.max(10000)]],
    beds: [0, [Validators.required, Validators.min(0), Validators.max(20)]],
    checkInHour: [14, [Validators.required, Validators.min(0), Validators.max(23)]],
    checkOutHour: [11, [Validators.required, Validators.min(0), Validators.max(23)]],
    squareFeet: [100, [Validators.required, Validators.min(10), Validators.max(10000)]],
    imageUrl: ['', [Validators.maxLength(500)]],
    cityId: [0, [Validators.required]],
    cityName: ['', [Validators.maxLength(100)]],
    amenityIds: [[] as number[]],
    features: this.fb.nonNullable.group({
      internet: [false],
      airConditioned: [false],
      petsAllowed: [false],
      parking: [false],
      entertainment: [false],
      kitchen: [false],
      refrigerator: [false],
      washer: [false],
      dryer: [false],
      selfCheckIn: [false],
    }),
  });

  ngOnInit() {
    this.loadAdminData();
  }

  loadAdminData() {
    this.error.set(null);
    this.placesService.loadAll().subscribe({
      error: (err) => this.error.set(err?.message ?? 'Unable to load places.'),
    });

    this.placesService.options().subscribe({
      next: (options) => this.options.set(options),
      error: (err) => this.error.set(err?.message ?? 'Unable to load form options.'),
    });
  }

  editPlace(place: PlacePreview) {
    this.selectedPlace.set(place);
    this.form.reset({
      name: place.name,
      details: place.details ?? '',
      guestCapacity: place.guestCapacity,
      rate: place.rate,
      beds: place.beds,
      checkInHour: 14,
      checkOutHour: 11,
      squareFeet: 100,
      imageUrl: place.imageUrl ?? '',
      cityId: place.cityId,
      cityName: '',
      amenityIds: place.amenityIds ?? [],
      features: place.features,
    });

    this.placesService.byId(place.id).subscribe({
      next: (details) => {
        this.form.patchValue({
          checkInHour: details.checkInHour,
          checkOutHour: details.checkOutHour,
          squareFeet: details.squareFeet,
          cityId: details.cityId,
          cityName: '',
          amenityIds: details.amenityIds ?? [],
          features: details.features,
        });
      },
      error: (err) => this.error.set(err?.message ?? 'Unable to load place details.'),
    });
  }

  startCreate() {
    this.selectedPlace.set(null);
    this.form.reset({
      name: '',
      details: '',
      guestCapacity: 1,
      rate: 0,
      beds: 0,
      checkInHour: 14,
      checkOutHour: 11,
      squareFeet: 100,
      imageUrl: '',
      cityId: this.options().cities[0]?.id ?? 0,
      cityName: '',
      amenityIds: [],
      features: {
        internet: false,
        airConditioned: false,
        petsAllowed: false,
        parking: false,
        entertainment: false,
        kitchen: false,
        refrigerator: false,
        washer: false,
        dryer: false,
        selfCheckIn: false,
      },
    });
  }

  toggleAmenity(id: number, checked: boolean) {
    const current = this.form.controls.amenityIds.value;
    const next = checked
      ? Array.from(new Set([...current, id]))
      : current.filter(item => item !== id);

    this.form.controls.amenityIds.setValue(next);
    this.form.controls.amenityIds.markAsDirty();
  }

  hasAmenity(id: number) {
    return this.form.controls.amenityIds.value.includes(id);
  }

  save() {
    const cityNameControl = this.form.controls.cityName;
    if (this.form.controls.cityId.value === 0 && !cityNameControl.value.trim()) {
      cityNameControl.setErrors({ ...cityNameControl.errors, required: true });
    } else if (cityNameControl.hasError('required')) {
      const { required, ...errors } = cityNameControl.errors ?? {};
      cityNameControl.setErrors(Object.keys(errors).length ? errors : null);
    }

    if (this.form.invalid || this.saving()) {
      this.form.markAllAsTouched();
      return;
    }

    const selected = this.selectedPlace();
    const request = this.toRequest();

    this.saving.set(true);
    this.error.set(null);

    const action = selected
      ? this.placesService.update(selected.id, { ...request, id: selected.id })
      : this.placesService.create(request);

    action.subscribe({
      next: (place) => {
        this.saving.set(false);
        this.refreshOptions();
        this.startCreate();
        this.selectedPlace.set(null);
      },
      error: (err) => {
        this.saving.set(false);
        this.error.set(err?.message ?? 'Unable to save place.');
      },
    });
  }

  deletePlace(place: PlacePreview) {
    this.placePendingDelete.set(place);
  }

  cancelDelete() {
    this.placePendingDelete.set(null);
  }

  confirmDelete() {
    const place = this.placePendingDelete();
    if (!place) return;

    this.error.set(null);
    this.placesService.delete(place.id).subscribe({
      next: () => {
        this.placePendingDelete.set(null);
        if (this.selectedPlace()?.id === place.id) {
          this.startCreate();
        }
      },
      error: (err) => {
        this.placePendingDelete.set(null);
        this.error.set(err?.message ?? 'Unable to delete place.');
      },
    });
  }

  private toRequest(): PlaceFormRequest {
    const raw = this.form.getRawValue();

    return {
      name: raw.name.trim(),
      details: raw.details.trim() || null,
      guestCapacity: Number(raw.guestCapacity),
      rate: Number(raw.rate),
      beds: Number(raw.beds),
      checkInHour: Number(raw.checkInHour),
      checkOutHour: Number(raw.checkOutHour),
      squareFeet: Number(raw.squareFeet),
      imageUrl: raw.imageUrl.trim() || null,
      cityId: Number(raw.cityId),
      cityName: Number(raw.cityId) === 0 ? raw.cityName.trim() : null,
      amenityIds: raw.amenityIds,
      features: raw.features,
    };
  }

  private refreshOptions() {
    this.placesService.options().subscribe({
      next: (options) => this.options.set(options),
      error: () => void 0,
    });
  }
}
