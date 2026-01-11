import { CommonModule } from '@angular/common';
import { Component, computed, signal } from '@angular/core';

import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { HeroComponent } from "./hero/hero.component";

type PromoCard = {
  title: string;
  subtitle: string;
  priceText?: string;
  meta?: string;
  imageUrl: string;
  href?: string; // по-късно ще стане routerLink към detail page
};

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, MatButtonModule, MatCardModule, MatIconModule, HeroComponent],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss'
})
export class HomeComponent {
  cards: PromoCard[] = [
    {
      title: 'New Luxury Penthouse',
      subtitle: 'Anaheim, California',
      priceText: '$122 / night',
      meta: '★★★★★ (2 reviews)',
      imageUrl: 'https://images.unsplash.com/photo-1560448204-603b3fc33ddc?auto=format&fit=crop&w=1600&q=80',
    },
    {
      title: 'BBQ Patio • Smart TV • 20min Beach',
      subtitle: 'Fountain Valley, California',
      priceText: '$130 / night',
      meta: '★★★★★ (1 review)',
      imageUrl: 'https://images.unsplash.com/photo-1505691938895-1758d7feb511?auto=format&fit=crop&w=1600&q=80',
    },
  ];
}
