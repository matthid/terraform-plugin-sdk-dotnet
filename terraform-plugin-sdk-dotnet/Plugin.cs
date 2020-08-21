using System;
using System.Collections.Generic;
using System.Text;

namespace Matthid.TerraformSdk
{
    // SchemaConfigMode is used to influence how a schema item is mapped into a
    // corresponding configuration construct, using the ConfigMode field of
    // Schema.
    using SchemaConfigMode = System.Int32;
    using SchemaOrResource = System.Object;

    public class UnknownObject
    {

    }

    /// <summary>
    /// SchemaDiffSuppressFunc is a function which can be used to determine
    /// whether a detected diff on a schema element is "valid" or not, and
    /// suppress it from the plan if necessary.
    ///
    /// Return true if the diff should be suppressed, false to retain it.
    /// </summary>
    /// <param name="d"></param>
    /// <param name="old"></param>
    /// <param name="n"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public delegate bool SchemaDiffSuppressFunc(UnknownObject d, UnknownObject old, string n, ResourceData data);
    /// <summary>
    /// SchemaDefaultFunc is a function called to return a default value for
    /// a field.
    /// </summary>
    /// <param name="d"></param>
    /// <param name="error"></param>
    public delegate void SchemaDefaultFunc(UnknownObject d, UnknownObject error);

    /// <summary>
    /// Schema is used to describe the structure of a value.
    ///
    /// Read the documentation of the struct elements for important details.
    /// </summary>
    /// <remarks>
    /// https://github.com/hashicorp/terraform-plugin-sdk/blob/v2.0.1/helper/schema/schema.go
    /// </remarks>
    public class Schema
    {
        /// <summary>
        /// Type is the type of the value and must be one of the ValueType values.
        ///
        /// This type not only determines what type is expected/valid in configuring
        /// this value, but also what type is returned when ResourceData.Get is
        /// called. The types returned by Get are:
        ///
        ///   TypeBool - bool
        ///   TypeInt - int
        ///   TypeFloat - float64
        ///   TypeString - string
        ///   TypeList - []interface{}
        ///   TypeMap - map[string]interface{}
        ///   TypeSet - *schema.Set
        /// </summary>
        TypeSpec Type { get; }

        /// <summary>
        /// ConfigMode allows for overriding the default behaviors for mapping
        /// schema entries onto configuration constructs.
        ///
        /// By default, the Elem field is used to choose whether a particular
        /// schema is represented in configuration as an attribute or as a nested
        /// block; if Elem is a *schema.Resource then it's a block and it's an
        /// attribute otherwise.
        ///
        /// If Elem is *schema.Resource then setting ConfigMode to
        /// SchemaConfigModeAttr will force it to be represented in configuration
        /// as an attribute, which means that the Computed flag can be used to
        /// provide default elements when the argument isn't set at all, while still
        /// allowing the user to force zero elements by explicitly assigning an
        /// empty list.
        ///
        /// When Computed is set without Optional, the attribute is not settable
        /// in configuration at all and so SchemaConfigModeAttr is the automatic
        /// behavior, and SchemaConfigModeBlock is not permitted.
        /// </summary>
        SchemaConfigMode ConfigMode { get; }

        /// <summary>
        /// If one of these is set, then this item can come from the configuration.
        /// Both cannot be set. If Optional is set, the value is optional. If
        /// Required is set, the value is required.
        ///
        /// One of these must be set if the value is not computed. That is:
        /// value either comes from the config, is computed, or is both.
        /// </summary>
        bool Optional { get; }
        /// <inheritdoc cref="Optional"/>
        bool Required { get; }

        /// <summary>
        /// If this is non-nil, the provided function will be used during diff
        /// of this field. If this is nil, a default diff for the type of the
        /// schema will be used.
        ///
        /// This allows comparison based on something other than primitive, list
        /// or map equality - for example SSH public keys may be considered
        /// equivalent regardless of trailing whitespace.
        /// </summary>
        SchemaDiffSuppressFunc DiffSuppressFunc { get; }

        /// <summary>
        /// If this is non-nil, then this will be a default value that is used
        /// when this item is not set in the configuration.
        ///
        /// DefaultFunc can be specified to compute a dynamic default.
        /// Only one of Default or DefaultFunc can be set. If DefaultFunc is
        /// used then its return value should be stable to avoid generating
        /// confusing/perpetual diffs.
        ///
        /// Changing either Default or the return value of DefaultFunc can be
        /// a breaking change, especially if the attribute in question has
        /// ForceNew set. If a default needs to change to align with changing
        /// assumptions in an upstream API then it may be necessary to also use
        /// the MigrateState function on the resource to change the state to match,
        /// or have the Read function adjust the state value to align with the
        /// new default.
        ///
        /// If Required is true above, then Default cannot be set. DefaultFunc
        /// can be set with Required. If the DefaultFunc returns nil, then there
        /// will be no default and the user will be asked to fill it in.
        ///
        /// If either of these is set, then the user won't be asked for input
        /// for this key if the default is not nil.
        /// </summary>
        UnknownObject Default { get; }
        /// <inheritdoc cref="Default"/>
        SchemaDefaultFunc DefaultFunc { get; }

        /// <summary>
        /// Description is used as the description for docs, the language server and
        /// other user facing usage. It can be plain-text or markdown depending on the
        /// global DescriptionKind setting.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// InputDefault is the default value to use for when inputs are requested.
        /// This differs from Default in that if Default is set, no input is
        /// asked for. If Input is asked, this will be the default value offered.
        /// </summary>
        string InnerDefault { get; }

        /// <summary>
        /// The fields below relate to diffs.
        ///
        /// If Computed is true, then the result of this value is computed
        /// (unless specified by config) on creation.
        ///
        /// If ForceNew is true, then a change in this resource necessitates
        /// the creation of a new resource.
        ///
        /// StateFunc is a function called to change the value of this before
        /// storing it in the state (and likewise before comparing for diffs).
        /// The use for this is for example with large strings, you may want
        /// to simply store the hash of it.
        /// </summary>
        bool Computed { get; }
        /// <inheritdoc cref="Computed"/>
        bool ForceNew { get; }
        /// <inheritdoc cref="Computed"/>
        SchemaStateFunc StateFunc { get; }

        /// <summary>
        /// The following fields are only set for a TypeList, TypeSet, or TypeMap.
        ///
        /// Elem represents the element type. For a TypeMap, it must be a *Schema
        /// with a Type that is one of the primitives: TypeString, TypeBool,
        /// TypeInt, or TypeFloat. Otherwise it may be either a *Schema or a
        /// *Resource. If it is *Schema, the element type is just a simple value.
        /// If it is *Resource, the element type is a complex structure,
        /// potentially managed via its own CRUD actions on the API.
        /// </summary>
        SchemaOrResource Elem { get; }

        /// <summary>
        /// The following fields are only set for a TypeList or TypeSet.
        ///
        /// MaxItems defines a maximum amount of items that can exist within a
        /// TypeSet or TypeList. Specific use cases would be if a TypeSet is being
        /// used to wrap a complex structure, however more than one instance would
        /// cause instability.
        ///
        /// MinItems defines a minimum amount of items that can exist within a
        /// TypeSet or TypeList. Specific use cases would be if a TypeSet is being
        /// used to wrap a complex structure, however less than one instance would
        /// cause instability.
        ///
        /// If the field Optional is set to true then MinItems is ignored and thus
        /// effectively zero.
        /// </summary>
        int MaxItems { get; }
        /// <inheritdoc cref="MaxItems" />
        int MinItems { get; }

        /// <summary>
        /// The following fields are only valid for a TypeSet type.
        ///
        /// Set defines a function to determine the unique ID of an item so that
        /// a proper set can be built.
        /// </summary>
        SchemaSetFunc Set { get; }

        /// <summary>
        /// ComputedWhen is a set of queries on the configuration. Whenever any
        /// of these things is changed, it will require a recompute (this requires
        /// that Computed is set to true).
        ///
        /// NOTE: This currently does not work.
        /// </summary>
        string[] ComputedWhen { get; }

        /// <summary>
        /// ConflictsWith is a set of schema keys that conflict with this schema.
        /// This will only check that they're set in the _config_. This will not
        /// raise an error for a malfunctioning resource that sets a conflicting
        /// key.
        ///
        /// ExactlyOneOf is a set of schema keys that, when set, only one of the
        /// keys in that list can be specified. It will error if none are
        /// specified as well.
        ///
        /// AtLeastOneOf is a set of schema keys that, when set, at least one of
        /// the keys in that list must be specified.
        ///
        /// RequiredWith is a set of schema keys that must be set simultaneously.
        /// </summary>
        string[] ConflictsWith { get; }
        /// <inheritdoc cref="ConflictsWith"/>
        string[] ExactlyOneOf { get; }
        /// <inheritdoc cref="ConflictsWith"/>
        string[] AtLeastOneOf { get; }
        /// <inheritdoc cref="ConflictsWith"/>
        string[] RequiredWith { get; }

        /// <summary>
        /// When Deprecated is set, this attribute is deprecated.
        ///
        /// A deprecated field still works, but will probably stop working in near
        /// future. This string is the message shown to the user with instructions on
        /// how to address the deprecation.
        /// </summary>
        string Deprecated { get; }

        /// <summary>
        /// ValidateDiagFunc allows individual fields to define arbitrary validation
        /// logic. It is yielded the provided config value as an interface{} that is
        /// guaranteed to be of the proper Schema type, and it can yield diagnostics
        /// based on inspection of that value.
        ///
        /// ValidateDiagFunc is honored only when the schema's Type is set to TypeInt,
        /// TypeFloat, TypeString, TypeBool, or TypeMap. It is ignored for all other types.
        ///
        /// ValidateDiagFunc is also yielded the cty.Path the SDK has built up to this
        /// attribute. The SDK will automatically set the AttributePath of any returned
        /// Diagnostics to this path. Therefore the developer does not need to set
        /// the AttributePath for primitive types.
        ///
        /// In the case of TypeMap to provide the most precise information, please
        /// set an AttributePath with the additional cty.IndexStep:
        ///
        ///  AttributePath: cty.IndexStringPath("key_name")
        ///
        /// Or alternatively use the passed in path to create the absolute path:
        ///
        ///  AttributePath: append(path, cty.IndexStep{Key: cty.StringVal("key_name")})
        /// </summary>
        SchemaValidateDiagFunc ValidateDiagFunc { get; }

        /// <summary>
        /// Sensitive ensures that the attribute's value does not get displayed in
        /// logs or regular output. It should be used for passwords or other
        /// secret fields. Future versions of Terraform may encrypt these
        /// values.
        /// </summary>
        bool Sensitive { get; }
    }

    /// <summary>
    /// SchemaSetFunc is a function that must return a unique ID for the given
    /// element. This unique ID is used to store the element in a hash.
    /// </summary>
    public delegate int SchemaSetFunc(UnknownObject input);

    /// <summary>
    /// SchemaStateFunc is a function used to convert some type to a string
    /// to be stored in the state.
    /// </summary>
    public delegate string SchemaStateFunc(UnknownObject input);

    /// <summary>
    /// SchemaValidateDiagFunc is a function used to validate a single field in the
    /// schema and has Diagnostic support.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    public delegate Diagnostics SchemaValidateDiagFunc(UnknownObject input, Path path);

    public class Path
    {

    }

    /// <summary>
    /// Resource represents a thing in Terraform that has a set of configurable
    /// attributes and a lifecycle (create, read, update, delete).
    ///
    /// The Resource schema is an abstraction that allows provider writers to
    /// worry only about CRUD operations while off-loading validation, diff
    /// generation, etc. to this higher level library.
    ///
    /// In spite of the name, this struct is not used only for terraform resources,
    /// but also for data sources. In the case of data sources, the Create,
    /// Update and Delete functions must not be provided.
    /// </summary>
    /// <remarks>https://github.com/hashicorp/terraform-plugin-sdk/blob/v2.0.1/helper/schema/resource.go</remarks>
    public class Resource
    {
        /// <summary>
        /// Schema is the schema for the configuration of this resource.
        ///
        /// The keys of this map are the configuration keys, and the values
        /// describe the schema of the configuration value.
        ///
        /// The schema is used to represent both configurable data as well
        /// as data that might be computed in the process of creating this
        /// resource.
        /// </summary>
        IReadOnlyDictionary<string, Schema> Schema { get; }

        /// <summary>
        /// SchemaVersion is the version number for this resource's Schema
        /// definition. The current SchemaVersion stored in the state for each
        /// resource. Provider authors can increment this version number
        /// when Schema semantics change. If the State's SchemaVersion is less than
        /// the current SchemaVersion, the InstanceState is yielded to the
        /// MigrateState callback, where the provider can make whatever changes it
        /// needs to update the state to be compatible to the latest version of the
        /// Schema.
        ///
        /// When unset, SchemaVersion defaults to 0, so provider authors can start
        /// their Versioning at any integer >= 1
        /// </summary>
        int SchemaVersion { get; }


        /// <summary>
        /// StateUpgraders contains the functions responsible for upgrading an
        /// existing state with an old schema version to a newer schema. It is
        /// called specifically by Terraform when the stored schema version is less
        /// than the current SchemaVersion of the Resource.
        ///
        /// StateUpgraders map specific schema versions to a StateUpgrader
        /// function. The registered versions are expected to be ordered,
        /// consecutive values. The initial value may be greater than 0 to account
        /// for legacy schemas that weren't recorded and can be handled by
        /// MigrateState.
        /// </summary>
        StateUpgrader[] StateUpgraders { get; }

        /// <summary>
        /// The functions below are the CRUD operations for this resource.
        ///
        /// The only optional operation is Update. If Update is not
        /// implemented, then updates will not be supported for this resource.
        ///
        /// The ResourceData parameter in the functions below are used to
        /// query configuration and changes for the resource as well as to set
        /// the ID, computed data, etc.
        ///
        /// The interface{} parameter is the result of the ConfigureFunc in
        /// the provider for this resource. If the provider does not define
        /// a ConfigureFunc, this will be nil. This parameter should be used
        /// to store API clients, configuration structures, etc.
        ///
        /// These functions are passed a context configured to timeout with whatever
        /// was set as the timeout for this operation. Useful for forwarding on to
        /// backend SDK's that accept context. The context will also cancel if
        /// Terraform sends a cancellation signal.
        ///
        /// These functions return diagnostics, allowing developers to build
        /// a list of warnings and errors to be presented to the Terraform user.
        /// The AttributePath of those diagnostics should be built within these
        /// functions, please consult go-cty documentation for building a cty.Path
        /// </summary>
        CreateContextFunc CreateContext { get; }
        /// <inheritdoc cref="CreateContext"/>
        ReadContextFunc ReadContext { get; }
        /// <inheritdoc cref="CreateContext"/>
        UpdateContextFunc UpdateContext { get; }
        /// <inheritdoc cref="CreateContext"/>
        DeleteContextFunc DeleteContext { get; }

        /// <summary>
        /// CustomizeDiff is a custom function for working with the diff that
        /// Terraform has created for this resource - it can be used to customize the
        /// diff that has been created, diff values not controlled by configuration,
        /// or even veto the diff altogether and abort the plan. It is passed a
        /// *ResourceDiff, a structure similar to ResourceData but lacking most write
        /// functions like Set, while introducing new functions that work with the
        /// diff such as SetNew, SetNewComputed, and ForceNew.
        ///
        /// The phases Terraform runs this in, and the state available via functions
        /// like Get and GetChange, are as follows:
        ///
        ///  * New resource: One run with no state
        ///  * Existing resource: One run with state
        ///   * Existing resource, forced new: One run with state (before ForceNew),
        ///     then one run without state (as if new resource)
        ///  * Tainted resource: No runs (custom diff logic is skipped)
        ///  * Destroy: No runs (standard diff logic is skipped on destroy diffs)
        ///
        /// This function needs to be resilient to support all scenarios.
        ///
        /// For the most part, only computed fields can be customized by this
        /// function.
        ///
        /// This function is only allowed on regular resources (not data sources).
        /// </summary>
        CustomizeDiffFunc CustomizeDiff { get; }

        /// <summary>
        /// Importer is the ResourceImporter implementation for this resource.
        /// If this is nil, then this resource does not support importing. If
        /// this is non-nil, then it supports importing and ResourceImporter
        /// must be validated. The validity of ResourceImporter is verified
        /// by InternalValidate on Resource.
        /// </summary>
        ResourceImporter Importer { get; }

        /// <summary>
        /// If non-empty, this string is emitted as a warning during Validate.
        /// </summary>
        string DeprecationMessage { get; }

        /// <summary>
        /// Timeouts allow users to specify specific time durations in which an
        /// operation should time out, to allow them to extend an action to suit their
        /// usage. For example, a user may specify a large Creation timeout for their
        /// AWS RDS Instance due to it's size, or restoring from a snapshot.
        /// Resource implementors must enable Timeout support by adding the allowed
        /// actions (Create, Read, Update, Delete, Default) to the Resource struct, and
        /// accessing them in the matching methods.
        /// </summary>
        ResourceTimeout Timeouts { get; }

        /// <summary>
        /// Description is used as the description for docs, the language server and
        /// other user facing usage. It can be plain-text or markdown depending on the
        /// global DescriptionKind setting.
        /// </summary>
        string Description { get; }
    }
    /// <summary>
    /// See Resource documentation.
    /// </summary>
    public delegate Diagnostics CreateContextFunc(Context xtc, ResourceData resourceData, UnknownObject state);
    /// <summary>
    /// See Resource documentation.
    /// </summary>
    public delegate Diagnostics ReadContextFunc(Context xtc, ResourceData resourceData, UnknownObject state);
    /// <summary>
    /// See Resource documentation.
    /// </summary>
    public delegate Diagnostics UpdateContextFunc(Context xtc, ResourceData resourceData, UnknownObject state);
    /// <summary>
    /// See Resource documentation.
    /// </summary>
    public delegate Diagnostics DeleteContextFunc(Context xtc, ResourceData resourceData, UnknownObject state);
    /// <summary>
    /// See Resource documentation.
    /// </summary>
    public delegate (InstanceState, Error) StateMigrateFunc(int version, InstanceState state, UnknownObject s);

    public delegate Error CustomizeDiffFunc(Context ctx, ResourceDiff diff, UnknownObject data);

    /// <summary>
    /// 
    /// </summary>
    /// <remarks>https://github.com/hashicorp/terraform-plugin-sdk/blob/v2.0.1/helper/schema/resource_timeout.go</remarks>
    public class ResourceTimeout
    {

    }

    public class ResourceImporter
    {

    }

    public class ResourceDiff
    {

    }

    public class Error
    {

    }
    public class InstanceState
    {

    }

    public class StateUpgrader
    {
        /// <summary>
        /// Version is the version schema that this Upgrader will handle, converting
        /// it to Version+1.
        /// </summary>
        public int Version { get; }

        /// <summary>
        /// Type describes the schema that this function can upgrade. Type is
        /// required to decode the schema if the state was stored in a legacy
        /// flatmap format.
        /// </summary>
        public TypeSpec Type { get; }

        /// <summary>
        /// Upgrade takes the JSON encoded state and the provider meta value, and
        /// upgrades the state one single schema version. The provided state is
        /// deocded into the default json types using a map[string]interface{}. It
        /// is up to the StateUpgradeFunc to ensure that the returned value can be
        /// encoded using the new schema.
        /// </summary>
        public StateUpgradeFunc Upgrade { get; }
    }

    /// <summary>See <see cref="StateUpgrader"/></summary>
    public delegate (IDictionary<string, object>, Error) StateUpgradeFunc(Context ctx, IReadOnlyDictionary<string, object> rawState, UnknownObject meta);

    public class Diagnostics
    {

    }
    public class Context
    {

    }

    public class ResourceData
    {

    }

    /// <summary>
    /// ConfigureContextFunc is the function used to configure a Provider.
    ///
    /// The interface{} value returned by this function is stored and passed into
    /// the subsequent resources as the meta parameter. This return value is
    /// usually used to pass along a configured API client, a configuration
    /// structure, etc.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="resourceData"></param>
    /// <returns></returns>
    public delegate (object re, Diagnostics diag) ConfigureContextFunc(Context context, ResourceData resourceData);

    /// <summary>
    /// Provider represents a resource provider in Terraform, and properly
    /// implements all of the ResourceProvider API.
    ///
    /// By defining a schema for the configuration of the provider, the
    /// map of supporting resources, and a configuration function, the schema
    /// framework takes over and handles all the provider operations for you.
    ///
    /// After defining the provider structure, it is unlikely that you'll require any
    /// of the methods on Provider itself.
    /// </summary>
    /// <remarks>
    /// https://github.com/hashicorp/terraform-plugin-sdk/blob/v2.0.1/helper/schema/provider.go
    /// </remarks>
    public class Provider
    {
        /// <summary>
        /// Schema is the schema for the configuration of this provider. If this
        /// provider has no configuration, this can be omitted.
        ///
        /// The keys of this map are the configuration keys, and the value is
        /// the schema describing the value of the configuration.
        /// </summary>
        IReadOnlyDictionary<string, Schema> Schema { get; }
        /// <summary>
        /// ResourcesMap is the list of available resources that this provider
        /// can manage, along with their Resource structure defining their
        /// own schemas and CRUD operations.
        ///
        /// Provider automatically handles routing operations such as Apply,
        /// Diff, etc. to the proper resource.
        /// </summary>
        IReadOnlyDictionary<string, Resource> ResourcesMap { get; }
        /// <summary>
        /// DataSourcesMap is the collection of available data sources that
        /// this provider implements, with a Resource instance defining
        /// the schema and Read operation of each.
        ///
        /// Resource instances for data sources must have a Read function
        /// and must *not* implement Create, Update or Delete.
        /// </summary>
        IReadOnlyDictionary<string, Resource> DataSourcesMap { get; }
        /// <summary>
        /// ProviderMetaSchema is the schema for the configuration of the meta
        /// information for this provider. If this provider has no meta info,
        /// this can be omitted. This functionality is currently experimental
        /// and subject to change or break without warning; it should only be
        /// used by providers that are collaborating on its use with the
        /// Terraform team.
        /// </summary>
        IReadOnlyDictionary<string, Schema> ProviderMetaSchema { get; }

        /// <summary>
        /// ConfigureContextFunc is a function for configuring the provider. If the
        /// provider doesn't need to be configured, this can be omitted. This function
        /// receives a context.Context that will cancel when Terraform sends a
        /// cancellation signal. This function can yield Diagnostics
        /// </summary>
        ConfigureContextFunc ConfigureContextFunc { get; }

        object meta { get; }
        string TerraformVersion { get; }
    }

    public delegate Provider ProviderFunc();

    public class ServeOpts
    {
    }

    public class Plugin
    {
        public static async System.Threading.Tasks.Task Serve(ServeOpts opts)
        {

        }
    }
}
