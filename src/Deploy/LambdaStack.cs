using Amazon.CDK;
using Amazon.CDK.AWS.Lambda;

namespace Deploy;

class LambdaStack : Stack
{
    public LambdaStack(Construct scope, string id, IStackProps? props = null)
        : base(scope, id, props)
    {
        var lambdaPath = Path.GetFullPath(Path.Combine(System.Environment.CurrentDirectory, "..", "..", "..", "..", "lambdas"));

        var lambdaFn = new Function(this, "CreditCheck", new FunctionProps
        {
            Runtime = new Runtime("NODEJS_20_X"),
            Code = Code.FromAsset(lambdaPath),
            Handler = "creditbureau.score",
            Timeout = Duration.Seconds(30)
        });

        var urlConfig = new FunctionUrl(this, "CreditCheckUrl", new FunctionUrlProps
        {
            AuthType = FunctionUrlAuthType.NONE,
            Function = lambdaFn
        });
    }
}
