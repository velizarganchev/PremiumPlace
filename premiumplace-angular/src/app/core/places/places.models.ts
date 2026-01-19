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
    amenity: string[];
};

export type Place = {
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
};
