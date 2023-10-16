using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolverSubProject.Information;

public class QuestionData
{
    public List<dynamic> VisualElements { get; set; } = new();

    public QuestionData()
    {
        
    }

    public dynamic? GetVisualElementById(string id) => VisualElements.FirstOrDefault(v => v.Id == id);

    public bool WillPotentiallyIntersect((string, string) seg1Id, (string, string) seg2Id)
    {
        var seg1 = GetVisualElementById(seg1Id.Item1 + seg1Id.Item2);
        var seg2 = GetVisualElementById(seg2Id.Item1 + seg2Id.Item2);
        return seg1?.Formula.Intersects(seg2?.Formula);
    }
}
