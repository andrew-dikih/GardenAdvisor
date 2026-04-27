namespace GardenAdvisor.API.Services;
using GardenAdvisor.API.Models;

public class ClimateZoneService : IClimateZoneService
{
    public ClimateZone GetClimateZone(double latitude, double longitude)
    {
        var absLat = Math.Abs(latitude);
        string zoneCode;
        string zoneName;
        string description;
        double minTempF;
        double maxTempF;
        string koppen;
        int frostFreeDays;
        string lastFrost;
        string firstFrost;

        if (absLat >= 65)
        {
            zoneCode = "1"; zoneName = "Zone 1 – Subarctic";
            description = "Extremely cold winters with very short growing season.";
            minTempF = -60; maxTempF = -50; koppen = "ET/EF";
            frostFreeDays = 60; lastFrost = "June"; firstFrost = "August";
        }
        else if (absLat >= 60)
        {
            zoneCode = "2"; zoneName = "Zone 2 – Arctic/Subarctic";
            description = "Very cold winters; growing season under 90 days.";
            minTempF = -50; maxTempF = -40; koppen = "Dfc";
            frostFreeDays = 80; lastFrost = "May"; firstFrost = "September";
        }
        else if (absLat >= 57)
        {
            zoneCode = "3"; zoneName = "Zone 3 – Northern Continental";
            description = "Cold winters; short but productive growing season.";
            minTempF = -40; maxTempF = -30; koppen = "Dfb";
            frostFreeDays = 110; lastFrost = "May"; firstFrost = "September";
        }
        else if (absLat >= 54)
        {
            zoneCode = "4"; zoneName = "Zone 4 – Cold Temperate";
            description = "Cold winters; moderate summers; ~130 frost-free days.";
            minTempF = -30; maxTempF = -20; koppen = "Dfb";
            frostFreeDays = 130; lastFrost = "May"; firstFrost = "October";
        }
        else if (absLat >= 50)
        {
            zoneCode = "5"; zoneName = "Zone 5 – Cool Temperate";
            description = "Cold winters; warm summers; wide plant selection possible.";
            minTempF = -20; maxTempF = -10; koppen = "Dfb";
            frostFreeDays = 160; lastFrost = "April"; firstFrost = "October";
        }
        else if (absLat >= 46)
        {
            zoneCode = "6"; zoneName = "Zone 6 – Mild Temperate";
            description = "Moderate winters; warm summers; excellent growing conditions.";
            minTempF = -10; maxTempF = 0; koppen = "Dfa/Cfb";
            frostFreeDays = 180; lastFrost = "April"; firstFrost = "October";
        }
        else if (absLat >= 41)
        {
            zoneCode = "7"; zoneName = "Zone 7 – Warm Temperate";
            description = "Mild winters; hot summers; long growing season.";
            minTempF = 0; maxTempF = 10; koppen = "Cfa/Cfb";
            frostFreeDays = 210; lastFrost = "March"; firstFrost = "November";
        }
        else if (absLat >= 36)
        {
            zoneCode = "8"; zoneName = "Zone 8 – Subtropical";
            description = "Warm winters; hot humid summers; frost rare.";
            minTempF = 10; maxTempF = 20; koppen = "Cfa/Csa";
            frostFreeDays = 250; lastFrost = "March"; firstFrost = "November";
        }
        else if (absLat >= 30)
        {
            zoneCode = "9"; zoneName = "Zone 9 – Warm Subtropical";
            description = "Mild winters; very hot summers; year-round gardening possible.";
            minTempF = 20; maxTempF = 30; koppen = "Csa/BSh";
            frostFreeDays = 290; lastFrost = "February"; firstFrost = "December";
        }
        else if (absLat >= 25)
        {
            zoneCode = "10"; zoneName = "Zone 10 – Tropical Margin";
            description = "Very mild winters; tropical-like conditions; frost extremely rare.";
            minTempF = 30; maxTempF = 40; koppen = "Aw/BSh";
            frostFreeDays = 330; lastFrost = "January"; firstFrost = "December";
        }
        else
        {
            zoneCode = "11"; zoneName = "Zone 11 – Tropical";
            description = "No frost; year-round tropical growing season.";
            minTempF = 40; maxTempF = 50; koppen = "Af/Am";
            frostFreeDays = 365; lastFrost = "None"; firstFrost = "None";
        }

        // Adjust for southern hemisphere (seasons flipped)
        if (latitude < 0)
        {
            lastFrost = ShiftMonths(lastFrost, 6);
            firstFrost = ShiftMonths(firstFrost, 6);
        }

        return new ClimateZone
        {
            ZoneCode = zoneCode,
            ZoneName = zoneName,
            Description = description,
            MinTempF = minTempF,
            MaxTempF = maxTempF,
            KoppenClassification = koppen,
            AverageFrostFreeDays = frostFreeDays,
            LastFrostMonth = lastFrost,
            FirstFrostMonth = firstFrost
        };
    }

    private static string ShiftMonths(string month, int shiftBy)
    {
        if (month is "None") return month;
        var months = new[] { "January","February","March","April","May","June","July","August","September","October","November","December" };
        var idx = Array.IndexOf(months, month);
        if (idx < 0) return month;
        return months[(idx + shiftBy) % 12];
    }
}
