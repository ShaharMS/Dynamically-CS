using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamically.Backend;

public class ChangeListener
{
    public List<Action<double, double, double, double>> OnMove = new List<Action<double, double, double, double>>();
    public List<Action> OnChange = new List<Action>();
}
