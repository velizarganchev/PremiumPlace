import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';

import { MatButtonModule } from '@angular/material/button';

import { HeroComponent } from "./hero/hero.component";
import { CardsGridComponent } from "../../shared/ui/cards-grid/cards-grid.component";
import { PlacesService } from '../../core/places/places.service';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, MatButtonModule, HeroComponent, CardsGridComponent],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss'
})
export class HomeComponent {

  private placesService = inject(PlacesService);
  cards = this.placesService.cards;

  ngOnInit() {
    this.placesService.loadAll().subscribe();
  }
}
