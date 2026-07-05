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
    features: PlaceFeatures;
    amenitys: string[];
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
    rate: number;
    imageUrl: string;
    amenity: string[];
    features: PlaceFeatures;
    guestCapacity: number;
    beds: number;
    reviewSummary: ReviewSummary;
};
