import { CommonModule } from '@angular/common';
import { Component, computed, DestroyRef, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

import { MatButtonModule } from '@angular/material/button';

import { HeroComponent } from "./hero/hero.component";
import { CardsGridComponent } from "../../shared/ui/cards-grid/cards-grid.component";
import { PlacesService } from '../../core/places/places.service';
import { mapPlaceToCard } from '../../core/places/places.mapper';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, MatButtonModule, HeroComponent, CardsGridComponent],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss'
})
export class HomeComponent {

  private placesService = inject(PlacesService);
  private readonly destroyRef = inject(DestroyRef);
  cards = computed(() =>
    [...this.placesService.places()]
      .sort((a, b) =>
        b.reviewSummary.avg - a.reviewSummary.avg ||
        b.reviewSummary.count - a.reviewSummary.count ||
        a.id - b.id
      )
      .slice(0, 4)
      .map(mapPlaceToCard)
  );

  ngOnInit() {
    this.placesService.loadAll()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe();
  }
}
