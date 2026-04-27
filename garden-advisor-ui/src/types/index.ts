export interface LatLng {
  lat: number;
  lng: number;
}

export interface Address {
  displayName: string;
  latitude: number;
  longitude: number;
  street: string;
  city: string;
  state: string;
  country: string;
  postalCode: string;
}

export interface GardenArea {
  polygon: LatLng[];
  areaSquareMeters: number;
}

export interface PlantCategory {
  id: string;
  name: string;
  description: string;
  icon: string;
}

export interface Plant {
  id: string;
  name: string;
  category: string;
  description: string;
  imageUrl: string;
  growingZones: string[];
  daysToMaturity: number;
  sunRequirement: string;
  waterRequirement: string;
  spacingMeters: number;
}

export interface ClimateZone {
  zoneCode: string;
  zoneName: string;
  description: string;
  minTempF: number;
  maxTempF: number;
  koppenClassification: string;
  averageFrostFreeDays: number;
  lastFrostMonth: string;
  firstFrostMonth: string;
}

export interface PlantingScheduleItem {
  plant: Plant;
  startIndoorsMonth: string;
  transplantMonth: string;
  directSowMonth: string;
  harvestStartMonth: string;
  harvestEndMonth: string;
  notes: string;
  recommendedPlotPosition?: LatLng;
}

export interface PlantWarning {
  plant: Plant;
  reason: string;
}

export interface GardenDesignResult {
  sessionId: string;
  climateZone: ClimateZone;
  schedule: PlantingScheduleItem[];
  additionalRecommendations: Plant[];
  warnings: PlantWarning[];
  layoutImageUrl: string;
  generatedAt: string;
}

export interface GardenRequest {
  sessionId: string;
  address: Address;
  gardenArea: GardenArea;
  selectedPlantIds: string[];
  userEmail?: string;
  userId?: string;
}

export interface UserSession {
  sessionId: string;
  email?: string;
  name?: string;
}
