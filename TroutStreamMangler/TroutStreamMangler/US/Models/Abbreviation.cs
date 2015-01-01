namespace TroutStreamMangler.US
{
    public class Abbreviation
    {
        private string _stateName;
        private string _stateAbbreviation;

        public string State_name
        {
            get { return _stateName.Trim(); }
            set { _stateName = value; }
        }

        public string State_abbreviation
        {
            get { return _stateAbbreviation.Trim(); }
            set { _stateAbbreviation = value; }
        }
    }
}