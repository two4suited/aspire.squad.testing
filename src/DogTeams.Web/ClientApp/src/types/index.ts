export interface Team {
  id: string;
  name: string;
  description: string;
  createdAt: string;
}

export interface Owner {
  id: string;
  teamId: string;
  userId: string;
  name: string;
  email: string;
  dogs: Dog[];
  createdAt: string;
}

export interface Dog {
  id: string;
  ownerId: string;
  teamId: string;
  name: string;
  breed: string;
  dateOfBirth: string;
  createdAt: string;
}

export interface AuthUser {
  id: string;
  email: string;
  name: string;
  token: string;
  teamId?: string;
}
