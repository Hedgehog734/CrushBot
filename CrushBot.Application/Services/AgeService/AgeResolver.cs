namespace CrushBot.Application.Services.AgeService;

public static class AgeResolver
{
    public const int MinAge = 13;
    public const int MaxAge = 100;

    public const double MaleLowerScale = 0.5;
    public const double MaleLowerOffset = 21;
    public const double MaleUpperScale = -0.5;
    public const double MaleUpperOffset = 10;
    
    public const double FemaleUpperScale = 0.85;
    public const double FemaleUpperOffset = 17;

    public static (int, int) GetMaleAgeSuggested(int age)
    {
        return GetMaleAgeRange(age);
    }

    public static (int, int) GetMaleAgeBoundary(int age)
    {
        var lowerMod = age / 18d;
        var upperMod = age * 0.1;
        return GetMaleAgeRange(age, lowerMod, upperMod);
    }

    public static (int, int) GetFemaleAgeSuggested(int age)
    {
        return GetFemaleAgeRange(age);
    }

    public static (int, int) GetFemaleAgeBoundary(int age)
    {
        var lowerMod = Math.Log(age);
        var upperMod = Math.Log(age * 0.377);
        return GetFemaleAgeRange(age, lowerMod, upperMod);
    }

    private static (int, int) GetMaleAgeRange(int age, double lowerMod = 1, double upperMod = 1)
    {
        var lowerCoef = CalcMaleLowerCoef(age);
        var upperCoef = CalcMaleUpperCoef(age);

        return GetAgeRange(age, lowerCoef, upperCoef, lowerMod, upperMod);
    }

    private static (int, int) GetFemaleAgeRange(int age, double lowerMod = 1, double upperMod = 1)
    {
        var lowerCoef = age * 0.016;
        var upperCoef = CalcFemaleUpperCoef(age);
        return GetAgeRange(age, lowerCoef, upperCoef, lowerMod, upperMod);
    }

    private static (int, int) GetAgeRange(int age, double lowerCoef, double upperCoef, double lowerMod = 1,
        double upperMod = 1)
    {
        var lowerFactor = Math.Log(age) * lowerCoef * lowerMod;
        var lower = (int)Math.Round(age - lowerFactor);

        if (lower < MinAge)
        {
            lower = MinAge;
        }

        var upperFactor = Math.Log(age) * upperCoef * upperMod;
        var upper = (int)Math.Round(age + upperFactor);

        if (upper > MaxAge)
        {
            upper = MaxAge;
        }

        return (lower, upper);
    }

    private static double CalcMaleLowerCoef(int age)
    {
        return CalculateCoefficient(age, MaleLowerScale, MaleLowerOffset);
    }

    private static double CalcMaleUpperCoef(int age)
    {
        return CalculateCoefficient(age, MaleUpperScale, MaleUpperOffset);
    }

    private static double CalcFemaleUpperCoef(int age)
    {
        return CalculateCoefficient(age, FemaleUpperScale, FemaleUpperOffset);
    }

    private static double CalculateCoefficient(int age, double scale, double offset)
    {
        return 1.0 + 0.5 * Math.Tanh(scale * (age - offset));
    }
}