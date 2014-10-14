namespace ParsingTelNumbers.Config
{
    public class InfoHolder
    {
        public SiteEnum Site { get; set; }

        public DirectionEnum Direction { get; set; }

        public string Name { get; set; }
        public string City { get; set; }
        public string Phone { get; set; }
    }

    public enum DirectionEnum
    {
        moto,
        aqua,
        spare,
        equip
    }

    public enum SiteEnum
    {
        ria,
        motosale
    }
}