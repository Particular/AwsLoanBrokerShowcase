using Amazon.CDK;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Lambda;

namespace Deploy;

class LambdaStack : Stack
{
    public LambdaStack(Construct scope, string id, IStackProps? props = null)
        : base(scope, id, props)
    {
        var role = new Role(this, "CreditCheckRole", new RoleProps
        {
            InlinePolicies =
            {
                { "iam_for_loan_broker_lambda", new PolicyDocument(new PolicyDocumentProps()
                {
                    Statements =
                    [
                        new PolicyStatement(new PolicyStatementProps()
                        {
                            Actions = ["sts:AssumeRole"],
                            Principals = [new ServicePrincipal("lambda.amazonaws.com")],
                            Effect = Effect.ALLOW
                        })
                    ]
                })}
            }
        });

        var lambdaPath = Path.GetFullPath(Path.Combine(System.Environment.CurrentDirectory, "..", "..", "..", "..", "lambdas"));
        var lambdaFn = new Function(this, "CreditCheck", new FunctionProps
        {
            Runtime = new Runtime("NODEJS_20_X"),
            Code = Code.FromAsset(lambdaPath),
            Handler = "creditbureau.score",
            Timeout = Duration.Seconds(30),
            Role = role
        });

        _ = new FunctionUrl(this, "CreditCheckUrl", new FunctionUrlProps
        {
            AuthType = FunctionUrlAuthType.NONE,
            Function = lambdaFn
        });

        _ = new Permission()
        {
            FunctionUrlAuthType = FunctionUrlAuthType.NONE,
            Principal = new AccountPrincipal("*"),
            Action = "lambda:InvokeFunctionUrl",
            Scope = lambdaFn
        };
    }
}
