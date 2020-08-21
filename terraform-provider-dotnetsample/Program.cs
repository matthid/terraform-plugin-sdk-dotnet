using Google.Protobuf;
using Matthid.TerraformSdk;
using MessagePack;
using Newtonsoft.Json;
using Org.BouncyCastle.Crypto;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using grpc = global::Grpc.Core;

namespace terraform_net_sample
{
    [MessagePackObject]
    public class Test_Config
    {
        [Key("input")]
        public string input;
    }

    [MessagePackObject]
    public class Test_State
    {
        [Key("input")]
        public string input;
        [Key("output")]
        public string output;
    }


    public class ExampleProvisioner : Tfplugin5.Provisioner.ProvisionerBase
    {
        public override async Task<global::Tfplugin5.GetProvisionerSchema.Types.Response> GetSchema(global::Tfplugin5.GetProvisionerSchema.Types.Request request, grpc::ServerCallContext context)
        {
            throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, "Provisioner_GetSchema"));
        }

        public override async Task<global::Tfplugin5.ValidateProvisionerConfig.Types.Response> ValidateProvisionerConfig(global::Tfplugin5.ValidateProvisionerConfig.Types.Request request, grpc::ServerCallContext context)
        {
            throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, "Provisioner_ValidateProvisionerConfig"));
        }

        public override async Task ProvisionResource(global::Tfplugin5.ProvisionResource.Types.Request request, grpc::IServerStreamWriter<global::Tfplugin5.ProvisionResource.Types.Response> responseStream, grpc::ServerCallContext context)
        {
            throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, "Provisioner_ProvisionResource"));
        }

        public override async Task<global::Tfplugin5.Stop.Types.Response> Stop(global::Tfplugin5.Stop.Types.Request request, grpc::ServerCallContext context)
        {
            throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, "Provisioner_Stop"));
        }
    }

    public class ExampleProvider : Tfplugin5.Provider.ProviderBase
    {
        /// <summary>
        ///////// Information about what a provider supports/expects
        /// </summary>
        /// <param name="request">The request received from the client.</param>
        /// <param name="context">The context of the server-side call handler being invoked.</param>
        /// <returns>The response to send back to the client (wrapped by a task).</returns>
        public override async Task<global::Tfplugin5.GetProviderSchema.Types.Response> GetSchema(global::Tfplugin5.GetProviderSchema.Types.Request request, grpc::ServerCallContext context)
        {
            //throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
            var resp = new Tfplugin5.GetProviderSchema.Types.Response();

            var global_schema = new Tfplugin5.Schema();
            global_schema.Version = 1;
            var globlal_block = new Tfplugin5.Schema.Types.Block();
            globlal_block.Version = 1;
            globlal_block.Description = "Example block";
            globlal_block.DescriptionKind = Tfplugin5.StringKind.Plain;
            global_schema.Block = globlal_block;
            resp.Provider = global_schema;

            // _test ressource
            var schema = new Tfplugin5.Schema();
            schema.Version = 1;
            var block = new Tfplugin5.Schema.Types.Block();
            block.Version = 1;
            block.Description = "Test Ressource block";
            block.DescriptionKind = Tfplugin5.StringKind.Plain;

            var attribute = new Tfplugin5.Schema.Types.Attribute();
            attribute.Name = "input";
            attribute.Description = "The input attribute";
            attribute.DescriptionKind = Tfplugin5.StringKind.Plain;
            attribute.Type =
                ByteString.CopyFromUtf8(
                    new TypeSpec.Primitive(TypeSpec.Primitive.PrimitiveType.String)
                    .ToJson());
            block.Attributes.Add(attribute);

            var outputAttribute = new Tfplugin5.Schema.Types.Attribute();
            outputAttribute.Name = "output";
            outputAttribute.Computed = true;
            outputAttribute.Description = "The output attribute";
            outputAttribute.DescriptionKind = Tfplugin5.StringKind.Plain;
            outputAttribute.Type =
                ByteString.CopyFromUtf8(
                    new TypeSpec.Primitive(TypeSpec.Primitive.PrimitiveType.String)
                    .ToJson());
            block.Attributes.Add(outputAttribute);


            schema.Block = block;
            resp.ResourceSchemas.Add("dotnetsample_test", schema);
            //resp.ProviderMeta = schema;
            return resp;

        }

        public override async Task<global::Tfplugin5.PrepareProviderConfig.Types.Response> PrepareProviderConfig(global::Tfplugin5.PrepareProviderConfig.Types.Request request, grpc::ServerCallContext context)
        {
            var resp = new Tfplugin5.PrepareProviderConfig.Types.Response();
            var newState = DynamicValueHelper.Deserialize<Test_State>(request.Config);
            //newState.output = "Echo: " + newState.input;
            //resp.PreparedConfig = DynamicValueHelper.Serialize(newState);
            resp.PreparedConfig = request.Config;
            return resp;
        }

        public override async Task<global::Tfplugin5.ValidateResourceTypeConfig.Types.Response> ValidateResourceTypeConfig(global::Tfplugin5.ValidateResourceTypeConfig.Types.Request request, grpc::ServerCallContext context)
        {
            var resp = new Tfplugin5.ValidateResourceTypeConfig.Types.Response();
            return resp;
        }

        public override async Task<global::Tfplugin5.ValidateDataSourceConfig.Types.Response> ValidateDataSourceConfig(global::Tfplugin5.ValidateDataSourceConfig.Types.Request request, grpc::ServerCallContext context)
        {
            var resp = new Tfplugin5.ValidateDataSourceConfig.Types.Response();
            return resp;
        }

        public override async Task<global::Tfplugin5.UpgradeResourceState.Types.Response> UpgradeResourceState(global::Tfplugin5.UpgradeResourceState.Types.Request request, grpc::ServerCallContext context)
        {
            var resp = new Tfplugin5.UpgradeResourceState.Types.Response();
            var data = request.RawState.Json.ToStringUtf8();
            var state = JsonConvert.DeserializeObject<Test_State>(data);
            resp.UpgradedState = DynamicValueHelper.Serialize(state);
            return resp;
        }

        /// <summary>
        ///////// One-time initialization, called before other functions below
        /// </summary>
        /// <param name="request">The request received from the client.</param>
        /// <param name="context">The context of the server-side call handler being invoked.</param>
        /// <returns>The response to send back to the client (wrapped by a task).</returns>
        public override async Task<global::Tfplugin5.Configure.Types.Response> Configure(global::Tfplugin5.Configure.Types.Request request, grpc::ServerCallContext context)
        {
            var resp = new Tfplugin5.Configure.Types.Response();
            return resp;
        }

        /// <summary>
        ///////// Managed Resource Lifecycle
        /// </summary>
        /// <param name="request">The request received from the client.</param>
        /// <param name="context">The context of the server-side call handler being invoked.</param>
        /// <returns>The response to send back to the client (wrapped by a task).</returns>
        public override async Task<global::Tfplugin5.ReadResource.Types.Response> ReadResource(global::Tfplugin5.ReadResource.Types.Request request, grpc::ServerCallContext context)
        {
            var resp = new Tfplugin5.ReadResource.Types.Response();
            resp.NewState = request.CurrentState;
            return resp;
        }

        public override async Task<global::Tfplugin5.PlanResourceChange.Types.Response> PlanResourceChange(global::Tfplugin5.PlanResourceChange.Types.Request request, grpc::ServerCallContext context)
        {
            var resp = new Tfplugin5.PlanResourceChange.Types.Response();
            
            var config = DynamicValueHelper.Deserialize<Test_Config>(request.Config);
            var priorState = DynamicValueHelper.Deserialize<Test_State>(request.PriorState);
            var proposedState = DynamicValueHelper.Deserialize<Test_State>(request.ProposedNewState);
            var providerMetadata = DynamicValueHelper.Deserialize<Test_State>(request.ProviderMeta);

            proposedState.output = "Echo: " + proposedState.input;
            resp.PlannedState = DynamicValueHelper.Serialize(proposedState);
            var attrPath = new Tfplugin5.AttributePath();
            var step = new Tfplugin5.AttributePath.Types.Step();
            step.ElementKeyString = "output";
            attrPath.Steps.Add(step);
            resp.RequiresReplace.Add(attrPath);
            return resp;
        }

        public override async Task<global::Tfplugin5.ApplyResourceChange.Types.Response> ApplyResourceChange(global::Tfplugin5.ApplyResourceChange.Types.Request request, grpc::ServerCallContext context)
        {
            var resp = new Tfplugin5.ApplyResourceChange.Types.Response();

            var config = DynamicValueHelper.Deserialize<Test_Config>(request.Config);
            var priorState = DynamicValueHelper.Deserialize<Test_State>(request.PriorState);
            var plannedState = DynamicValueHelper.Deserialize<Test_State>(request.PlannedState);
            var providerMetadata = DynamicValueHelper.Deserialize<Test_State>(request.ProviderMeta);

            resp.NewState = DynamicValueHelper.Serialize(plannedState);
            return resp;
        }

        public override async Task<global::Tfplugin5.ImportResourceState.Types.Response> ImportResourceState(global::Tfplugin5.ImportResourceState.Types.Request request, grpc::ServerCallContext context)
        {
            throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, "ImportResourceState"));
        }

        public override async Task<global::Tfplugin5.ReadDataSource.Types.Response> ReadDataSource(global::Tfplugin5.ReadDataSource.Types.Request request, grpc::ServerCallContext context)
        {
            throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, "ReadDataSource"));
        }

        /// <summary>
        ///////// Graceful Shutdown
        /// </summary>
        /// <param name="request">The request received from the client.</param>
        /// <param name="context">The context of the server-side call handler being invoked.</param>
        /// <returns>The response to send back to the client (wrapped by a task).</returns>
        public override async Task<global::Tfplugin5.Stop.Types.Response> Stop(global::Tfplugin5.Stop.Types.Request request, grpc::ServerCallContext context)
        {
            var resp = new Tfplugin5.Stop.Types.Response();
            return resp;
        }

    }

    class Program
    {
        public const string ServiceHost = "127.0.0.1";
        // TODO: Find free port
        //public const int ServicePort = 52345;

        static int FreeTcpPort()
        {
            TcpListener l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            int port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            return port;
        }

        static async Task Main(string[] args)
        {
            const string caCertFile = "caCert.pem";
            const string caKeyFile = "caKey.pem";
            string caCertPem, caKeyPem;
            if (File.Exists(caCertFile) && File.Exists(caKeyFile))
            {
                caCertPem = File.ReadAllText(caCertFile);
                caKeyPem = File.ReadAllText(caKeyFile);
            }
            else
            {
                var (caCert, caKey) = CertsHelper.GenerateCACertificate("CN=localhost");
                CertsHelper.AddCertToStore(caCert, StoreName.Root, StoreLocation.CurrentUser);
                caCertPem = CertsHelper.CertificateToPem(caCert);
                caKeyPem = CertsHelper.PrivateToPem(caKey);
                File.WriteAllText(caCertFile, caCertPem);
                File.WriteAllText(caKeyFile, caKeyPem);
            }
            //var (serverCert, serverKey) = CertsHelper.GenerateSelfSignedCertificate("CN=127.0.0.1", "CN=MyROOTCA", caKey);

            //var chain = string.Join("", CertsHelper.CertificateToPem(caCert), CertsHelper.CertificateToPem(serverCert));
            var sslCerts = new grpc::SslServerCredentials(new grpc::KeyCertificatePair[]
            {
                new grpc::KeyCertificatePair(caCertPem, caKeyPem)
            });

            var health = new Grpc.HealthCheck.HealthServiceImpl();
            health.SetStatus("tfplugin5.Provider", Grpc.Health.V1.HealthCheckResponse.Types.ServingStatus.Serving);

            //health.SetStatus("plugin", Grpc.Health.V1.HealthCheckResponse.Types.ServingStatus.Serving);

            // Build a server to host the plugin over gRPC
            var ServicePort = FreeTcpPort();
            var server = new grpc::Server
            {
                Ports = { { ServiceHost, ServicePort, sslCerts } },
                Services = {
                    { Grpc.Health.V1.Health.BindService(health) },
                    { Tfplugin5.Provider.BindService(new ExampleProvider()) },
                    { Tfplugin5.Provisioner.BindService(new ExampleProvisioner()) },
                },
            };

            server.Start();

            // Part of the go-plugin handshake:
            //  https://github.com/hashicorp/go-plugin/blob/master/docs/guide-plugin-write-non-go.md#4-output-handshake-information
            await Console.Out.WriteAsync($"1|5|tcp|{ServiceHost}:{ServicePort}|grpc\n");
            await Console.Out.FlushAsync();

            while (Console.Read() == -1)
                await Task.Delay(1000);

            await server.ShutdownAsync();
        }
    }
}
