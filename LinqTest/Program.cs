using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqTest
{
	class Profile
	{
		public string Name { get; set; }
		public int Height { get; set; }

		public Profile(string name, int height)
		{
			Name = name;
			Height = height;
		}
	}

	class Product
	{
		public string Title { get; set; }
		public string Star { get; set; }
	}

	class Students
	{
		public string Name { get; set; }
		public List<int> Scores { get; set; }
	}

	class Program
	{
		static List<Profile> profiles = new List<Profile>
		{
			new Profile("정우성", 186),
			new Profile("김태희", 158),
			new Profile("고현정", 172),
			new Profile("이문세", 178),
			new Profile("하동훈", 171)
		};

		static List<Product> products = new List<Product>
		{
			new Product {Title = "비트", Star = "정우성"},
			new Product {Title = "아이리스", Star = "김태희"},
			new Product {Title = "카이스트", Star = "김태희"},
			new Product {Title = "모래시계", Star = "고현정"},
			new Product {Title = "솔로예찬", Star = "이문세"},
		};

		static List<Students> students = new List<Students>
		{
			new Students {Name = "문종헌", Scores = new List<int>{ 97, 72, 81, 60 }},
			new Students {Name = "송용직", Scores = new List<int>{ 75, 84, 91, 39 }},
			new Students {Name = "이문환", Scores = new List<int>{ 88, 94, 65, 85 }},
			new Students {Name = "김경현", Scores = new List<int>{ 97, 89, 85, 82 }},
		};

		static void Main(string[] args)
		{
			// 기본 Linq
			var sortedProfiles = from profile in profiles
								 where profile.Height < 175
								 orderby profile.Height
								 select new { Name = profile.Name, InchHeight = profile.Height * 0.393 };

			foreach (var profile in sortedProfiles)
			{
				Console.WriteLine($"{profile.Name}, {profile.InchHeight}");
			}

			// 중첩 from
			var scores = from student in students
						 from score in student.Scores
						 where 89 <= score
						 orderby score, student.Name
						 select new { Name = student.Name, Score = score };

			foreach (var student in scores)
			{
				Console.WriteLine($"{student.Name}, {student.Score}");
			}

			// group by
			var groups = from profile in profiles
						 group profile by profile.Height < 175 into g
						 select new { Key = g.Key, Profiles = g };

			foreach (var group in groups)
			{
				Console.WriteLine($"Under 175cm: {group.Key}");
				foreach (var profile in group.Profiles)
				{
					Console.WriteLine($"{profile.Name}, {profile.Height}");
				}
			}

			// inner join
			var innerJoinedProducts = from profile in profiles
									  join product in products
									  on profile.Name equals product.Star
									  select new { Name = profile.Name, Work = product.Title, Height = profile.Height };

			foreach (var v in innerJoinedProducts)
			{
				Console.WriteLine($"Name: {v.Name}, Work: {v.Work}, Height: {v.Height}");
			}

			// group join
			var groupJoinedProducts = from profile in profiles
									  join product in products
									  on profile.Name equals product.Star into productGroup
									  from product in productGroup
									  select new { Name = profile.Name, Work = product.Title, Height = profile.Height };

			foreach (var v in groupJoinedProducts)
			{
				Console.WriteLine($"Name: {v.Name}, Work: {v.Work}, Height: {v.Height}");
			}

			// outer join
			var outerJoinedProducts = from profile in profiles
									  join product in products
									  on profile.Name equals product.Star into productGroup
									  from product in productGroup.DefaultIfEmpty(new Product { Title = "N/A" })
									  select new { Name = profile.Name, Work = product.Title, Height = profile.Height };

			foreach (var v in outerJoinedProducts)
			{
				Console.WriteLine($"Name: {v.Name}, Work: {v.Work}, Height: {v.Height}");
			}
		}
	}
}
