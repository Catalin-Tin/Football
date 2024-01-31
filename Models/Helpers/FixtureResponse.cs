

    using System.Collections.Generic;

    namespace Football.Models.Helpers
    {
        public class FixtureResponse
        {
            public List<FixtureItem> Response { get; set; }
        }

        public class FixtureItem
        {
            public Fixture Fixture { get; set; }
         
            public FixtureTeams Teams { get; set; }
    }
 
        public class Fixture
    {
        public DateTime Date { get; set; }
        public Venue Venue {  set; get; }
    }
        public class Venue
        {
            public string City { get; set; }
        }

        public class FixtureTeams
        {
            public TeamDetails Home { get; set; }
            public TeamDetails Away { get; set; }
        }

        public class TeamDetails
        {
            public string Name { get; set; }
        }
    }


