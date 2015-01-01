namespace TroutStreamMangler
{
    public interface IDatabaseExporter
    {
        void ExportRestrictions();
        void ExportStreams();
        void ExportPubliclyAccessibleLand();
        void ExportLakes();
        void ExportPubliclyAccessibleLandSections();
        void ExportCountyToStreamRelations();
    }
}