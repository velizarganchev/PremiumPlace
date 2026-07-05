import { CardItem } from '../../shared/ui/cards-grid/cards-grid.model';
import type { PlacePreview, PlaceDto } from './places.models';

export const mapPlace = (dto: PlaceDto): PlacePreview => ({
    id: dto.id,
    name: dto.name,
    details: dto.details,
    city: dto.city,
    rate: dto.rate,
    imageUrl: dto.imageUrl,
    amenity: dto.amenitys ?? [],
    features: dto.features,
    guestCapacity: dto.guestCapacity,
    beds: dto.beds,
    reviewSummary: dto.reviewSummary ?? { avg: 0, count: 0 },
});

export const amenitiesPreview = (amenity: string[], take = 2) =>
    (amenity ?? []).slice(0, take).join(' • ');


export function mapPlaceToCard(place: PlacePreview): CardItem {
    return {
        id: String(place.id),
        title: place.name,
        subtitle: place.city,
        priceText: `EUR ${place.rate} / night`,
        meta: buildMeta(place),
        imageUrl: place.imageUrl,
        href: `/places/${place.id}`,
    };
}

function buildMeta(place: PlacePreview): string {
    const parts: string[] = [];

    parts.push(`${place.guestCapacity} guests`);
    parts.push(`${place.beds} beds`);

    if (place.features?.internet) {
        parts.push('Wi-Fi');
    }

    return parts.join(' • ');
}
