import { CommonModule } from '@angular/common';
import { Component, computed, signal } from '@angular/core';

import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

type HeroSlide = { imageUrl: string; title: string; subtitle?: string };

@Component({
  selector: 'app-hero',
  standalone: true,
  imports: [CommonModule, MatButtonModule, MatIconModule,],
  templateUrl: './hero.component.html',
  styleUrl: './hero.component.scss'
})
export class HeroComponent {
  slides: HeroSlide[] = [
    {
      imageUrl: 'https://images.unsplash.com/photo-1501183638710-841dd1904471?auto=format&fit=crop&w=2400&q=80',
      title: 'Discover your next place',
      subtitle: 'Clean, fast, and premium.',
    },
    {
      imageUrl: 'https://images.unsplash.com/photo-1505691938895-1758d7feb511?auto=format&fit=crop&w=2400&q=80',
      title: 'Book with confidence',
      subtitle: 'Smart search. Real-time availability.',
    },
    {
      imageUrl: 'https://images.unsplash.com/photo-1560448204-603b3fc33ddc?auto=format&fit=crop&w=2400&q=80',
      title: 'Premium stays',
      subtitle: 'Business, travel, family.',
    },
  ];

  activeIndex = signal(0);
  activeSlide = computed(() => this.slides[this.activeIndex()]);

  prev() {
    const next = (this.activeIndex() - 1 + this.slides.length) % this.slides.length;
    this.activeIndex.set(next);
  }

  next() {
    const next = (this.activeIndex() + 1) % this.slides.length;
    this.activeIndex.set(next);
  }

  go(i: number) {
    this.activeIndex.set(i);
  }
}
