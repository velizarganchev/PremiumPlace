export type User = {
    id: number;
    username: string;
    email: string;
    role: UserRole;
};

export type UserRole = 'User' | 'Admin';

export type LoginRequest = { usernameOrEmail: string; password: string };
export type RegisterRequest = { username: string; email: string; password: string };

export type AuthResponse = {
    user: User;
};
