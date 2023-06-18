using System.Text;
namespace StringJoinTest;
class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");
		Doc doc = new();
		doc.desc = "New Documents";
		Console.WriteLine(doc.ToString());
	}
}

internal class Doc
{
	public long id;
	public string name = null;
	public string desc;

	public override string ToString()
    {
		StringBuilder sb = new(1024);
		sb.AppendJoin('│',
			id,
			name,
            desc);
		return sb.ToString();
	}
}