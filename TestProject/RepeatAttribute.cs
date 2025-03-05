using System.Reflection;
using Xunit.Sdk;

namespace TestProject;


[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]

public class RepeatAttribute: DataAttribute
{
    private readonly int _count;

    public RepeatAttribute(int count)
    {
        _count = count;
    }

    public override IEnumerable<object[]> GetData(MethodInfo testMethod)
    {
        for (int i = 0; i < _count; i++)
        {
            yield return new object[0];
        }
    }
}