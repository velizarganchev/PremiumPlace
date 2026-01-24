import { Component, input } from '@angular/core';
import { CardItem } from './cards-grid.model';
import { CommonModule } from '@angular/common';

import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-cards-grid',
  standalone: true,
  imports: [RouterLink, CommonModule, MatButtonModule, MatCardModule, MatIconModule,],
  templateUrl: './cards-grid.component.html',
  styleUrl: './cards-grid.component.scss'
})
export class CardsGridComponent {
  cards = input.required<CardItem[]>();
}
