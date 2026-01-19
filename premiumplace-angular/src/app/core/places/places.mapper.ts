import { CardItem } from '../../shared/ui/cards-grid/cards-grid.model';
import type { Place, PlaceDto } from './places.models';

export const mapPlace = (dto: PlaceDto): Place => ({
    id: dto.id,
    name: dto.name,
    details: dto.details,
    city: dto.city,
    rate: dto.rate,
    imageUrl: dto.imageUrl,
    amenity: dto.amenity ?? [],
    features: dto.features,
    guestCapacity: dto.guestCapacity,
    beds: dto.beds,
});

export const amenitiesPreview = (amenity: string[], take = 2) =>
    (amenity ?? []).slice(0, take).join(' • ');


export function mapPlaceToCard(place: Place): CardItem {
    return {
        id: String(place.id),
        title: place.name,
        subtitle: place.city,
        priceText: `$${place.rate} / night`,
        meta: buildMeta(place),
        imageUrl: place.imageUrl,
        href: `/places/${place.id}`,
    };
}

function buildMeta(place: Place): string {
    const parts: string[] = [];

    parts.push(`${place.guestCapacity} guests`);
    parts.push(`${place.beds} beds`);

    if (place.features?.internet) {
        parts.push('Wi-Fi');
    }

    return parts.join(' • ');
}