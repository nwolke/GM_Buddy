export interface Campaign {
  id: number;
  name: string;
  description?: string;
  gameSystemId: number;
  gameSystemName?: string;
  accountId?: number;
  createdAt?: string;
  updatedAt?: string;
}
