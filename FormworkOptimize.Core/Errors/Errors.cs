namespace FormworkOptimize.Core.Errors
{
    public static  class Errors
    {

        public static ValueLessThanZeroError LessThanZeroError(string valueName) =>
           new ValueLessThanZeroError(valueName);

        public static ValueEqualZeroError EqualZeroError(string valueName) =>
            new ValueEqualZeroError(valueName);

        public static FileNotFoundError FileNotFound(string filePath) => 
            new FileNotFoundError(filePath);

        public static Default3DViewNotFoundError Default3DViewNotFound =>
            new Default3DViewNotFoundError();

        public static InvalidPolygonError InvalidPolygon =>
            new InvalidPolygonError();

        public static InvalidOpeningError InvalidOpening => 
            new InvalidOpeningError();

        public static ShortBeamError ShortBeam =>
            new ShortBeamError();

        public static ShortPropTypeError ShortPropType => 
            new ShortPropTypeError();

        public static MaterialNotFoundError MaterialNotFound => 
            new MaterialNotFoundError();

    }
}
