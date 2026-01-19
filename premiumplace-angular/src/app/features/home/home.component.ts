import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';

import { MatButtonModule } from '@angular/material/button';

import { HeroComponent } from "./hero/hero.component";
import { CardItem } from '../../shared/ui/cards-grid/cards-grid.model';
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
  cards = signal<CardItem[]>([]);

  ngOnInit() {
    this.placesService.loadAll().subscribe((places) => {

      this.cards.set(places.map(mapPlaceToCard));
    });
  }
}
