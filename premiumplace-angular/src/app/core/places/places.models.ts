export type PlaceFeatures = {
    internet: boolean;
    airConditioned: boolean;
    petsAllowed: boolean;
    parking: boolean;
    entertainment: boolean;
    kitchen: boolean;
    refrigerator: boolean;
    washer: boolean;
    dryer: boolean;
    selfCheckIn: boolean;
};


export type PlaceDto = {
    reviewSummary: {
        avg: number;
        count: number;
    };
    reviews: [
        {
            id: number;
            rating: number;
            comment: string;
            createdAt: string;
            userId: number;
            username: string;
        },
    ];
    id: number;
    name: string;
    details: string;
    guestCapacity: number;
    rate: number;
    beds: number;
    checkInHour: number;
    checkOutHour: number;
    squareFeet: number;
    imageUrl: string;
    city: string;
    cityId: number;
    features: PlaceFeatures;
    amenitys: string[];
    amenityIds: number[];
};

export type ReviewSummary = {
    avg: number;
    count: number;
};

export type PlacePreview = {
    id: number;
    name: string;
    details: string;
    city: string;
    cityId: number;
    rate: number;
    imageUrl: string;
    amenity: string[];
    amenityIds: number[];
    features: PlaceFeatures;
    guestCapacity: number;
    beds: number;
    reviewSummary: ReviewSummary;
};

export type PlaceFormRequest = {
    id?: number;
    name: string;
    details: string | null;
    guestCapacity: number;
    rate: number;
    beds: number;
    checkInHour: number;
    checkOutHour: number;
    squareFeet: number;
    imageUrl: string | null;
    cityId: number;
    cityName?: string | null;
    features: PlaceFeatures;
    amenityIds: number[];
};

export type PlaceSortKey = 'recommended' | 'priceAsc' | 'priceDesc' | 'capacityDesc';

export type PlaceQuery = {
    search?: string;
    city?: string;
    sort?: PlaceSortKey;
    page?: number;
    pageSize?: number;
};

export type PagedResult<T> = {
    items: T[];
    total: number;
    page: number;
    pageSize: number;
};

export type PlaceOptionItem = {
    id: number;
    name: string;
};

export type PlaceOptions = {
    cities: PlaceOptionItem[];
    amenities: PlaceOptionItem[];
};
