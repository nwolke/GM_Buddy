export interface Campaign {
  id: string;
  name: string;
  description?: string;
  gameSystemId: number;
  gameSystemName?: string;
  accountId?: number;
  createdAt?: string;
  updatedAt?: string;
}
