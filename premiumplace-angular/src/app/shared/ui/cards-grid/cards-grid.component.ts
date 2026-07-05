import { Component, inject, input } from '@angular/core';
import { CardItem } from './cards-grid.model';
import { CommonModule } from '@angular/common';

import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { Router } from '@angular/router';

@Component({
  selector: 'app-cards-grid',
  standalone: true,
  imports: [CommonModule, MatButtonModule, MatCardModule, MatIconModule,],
  templateUrl: './cards-grid.component.html',
  styleUrl: './cards-grid.component.scss'
})
export class CardsGridComponent {
  router = inject(Router);
  cards = input.required<CardItem[]>();

  onCardClick(card: CardItem): void {
    if (!card.href) return;
    this.router.navigateByUrl(card.href);
  }

  onCardActivate(event: Event, card: CardItem): void {
    // Prevent the space key from scrolling the page before navigating.
    event.preventDefault();
    this.onCardClick(card);
  }
}
