// Dates are exchanged as ISO date strings (yyyy-MM-dd) to match the backend DateOnly.

export type BlockedRange = { from: string; to: string };

export type Availability = { blockedRanges: BlockedRange[] };

export type CreateBookingRequest = {
    placeId: number;
    checkInDate: string;
    checkOutDate: string;
};

export type CreatePendingResult = { bookingId: number };

export type ConfirmBookingRequest = {
    bookingId: number;
    paymentReference: string;
};

export type ConfirmBookingResult = { bookingId: number; status: string };

export type MyBooking = {
    id: number;
    placeId: number;
    placeName: string;
    checkInDate: string;
    checkOutDate: string;
    status: string;
    createdAt: string;
    totalAmount: number;
    currencyCode: string;
};

export type PaymentConfig = { clientId: string; currency: string };
