/** Matches API JSON enum serialization (camelCase string names). */
export type DeviceType = 'Phone' | 'Tablet';

export interface Device {
  id: string;
  name: string;
  manufacturer: string;
  type: DeviceType;
  os: string;
  osVersion: string;
  processor: string;
  ramGb: number;
  description: string;
  location: string;
  assignedToUserId: string | null;
  createdAt: string;
  updatedAt: string | null;
  assignedAt: string | null;
}

/** Body for POST /api/devices/generate-description (no description/location yet). */
export interface GenerateDeviceDescriptionPayload {
  name: string;
  manufacturer: string;
  type: DeviceType;
  os: string;
  osVersion: string;
  processor: string;
  ramGb: number;
}

export interface DeviceWritePayload {
  name: string;
  manufacturer: string;
  type: DeviceType;
  os: string;
  osVersion: string;
  processor: string;
  ramGb: number;
  description: string;
  location: string;
  assignedToUserId: string | null;
}

export function deviceTypeLabel(t: DeviceType): string {
  return t === 'Phone' ? 'Phone' : 'Tablet';
}
