namespace TroutStreamMangler.US
{
    public class CensusFipsCodes
    {
        private string _state;
        private int _fipsCounty;
        private int _fipsState;
        private string _countyName;

        public string State
        {
            get { return _state.Trim(); }
            set { _state = value; }
        }

        public string County_Name
        {
            get { return _countyName.Trim(); }
            set { _countyName = value; }
        }

        public int FIPS_State
        {
            get { return _fipsState; }
            set { _fipsState = value; }
        }

        public int FIPS_County
        {
            get { return _fipsCounty; }
            set { _fipsCounty = value; }
        }
    }
}