using System.Collections.Generic;
using Deploy.Data;

namespace Deploy.IO
{
    internal static class DefaultData
    {
        public static readonly Dictionary<string, string[]> Sequences = new Dictionary<string, string[]>
        {
            {
                "AdminExecuteSequence", new[]
                {
                    "CostInitialize",
                    "FileCost",
                    "CostFinalize",
                    "InstallValidate",
                    "InstallInitialize",
                    "InstallAdminPackage",
                    "InstallFiles",
                    "InstallFinalize"
                }
            },
            {
                "AdvtExecuteSequence", new[]
                {
                    "CostInitialize",
                    "CostFinalize",
                    "InstallValidate",
                    "InstallInitialize",
                    "CreateShortcuts",
                    "PublishFeatures",
                    "PublishProduct",
                    "InstallFinalize"
                }
            },
            {
                "InstallExecuteSequence", new[]
                {
                    "FindRelatedProducts",
                    "LaunchConditions",
                    "ValidateProductID",
                    "CostInitialize",
                    "FileCost",
                    "CostFinalize",
                    "InstallValidate",
                    "InstallInitialize",
                    "RemoveExistingProducts",
                    "ProcessComponents",
                    "UnpublishFeatures",
                    "RemoveShortcuts",
                    "RemoveFiles",
                    "InstallFiles",
                    "CreateShortcuts",
                    "RegisterUser",
                    "RegisterProduct",
                    "PublishFeatures",
                    "PublishProduct",
                    "InstallFinalize"
                }
            }
        };

        public static readonly List<Table> Tables = new List<Table>
        {
            TableBuilder.Create("AdminExecuteSequence")
                .PrimaryColumn("Action")
                .Column("Condition")
                .Column("Sequence", DataType.Short)
                .Build(),

            TableBuilder.Create("AdvtExecuteSequence")
                .PrimaryColumn("Action")
                .Column("Condition")
                .Column("Sequence", DataType.Short)
                .Build(),

            TableBuilder.Create("Component")
                .PrimaryColumn("Component")
                .Column("ComponentId")
                .Column("Directory_", DataType.Char, Constraint.NotNull)
                .Column("Attributes", DataType.Short)
                .Column("Condition")
                .Column("KeyPath")
                .Build(),

            TableBuilder.Create("Directory")
                .PrimaryColumn("Directory")
                .Column("Directory_Parent")
                .Column("DefaultDir", DataType.Char, Constraint.NotNull)
                .Build(),

            TableBuilder.Create("Feature")
                .PrimaryColumn("Feature")
                .Column("Feature_Parent")
                .Column("Title")
                .Column("Description")
                .Column("Display", DataType.Short)
                .Column("Level", DataType.Short)
                .Column("Directory_")
                .Column("Attributes", DataType.Short)
                .Build(),

            TableBuilder.Create("FeatureComponents")
                .PrimaryColumn("Feature_")
                .PrimaryColumn("Component_")
                .Build(),

            TableBuilder.Create("File")
                .PrimaryColumn("File")
                .Column("Component_")
                .Column("FileName")
                .Column("FileSize", DataType.Long)
                .Column("Version")
                .Column("Language")
                .Column("Attributes", DataType.Short)
                .Column("Sequence", DataType.Short)
                .Build(),

            TableBuilder.Create("Icon")
                .PrimaryColumn("Name")
                .Column("Data", DataType.Object, Constraint.NotNull)
                .Build(),

            TableBuilder.Create("InstallExecuteSequence")
                .PrimaryColumn("Action")
                .Column("Condition")
                .Column("Sequence", DataType.Short)
                .Build(),

            TableBuilder.Create("LaunchCondition")
                .PrimaryColumn("Condition")
                .Column("Description")
                .Build(),

            TableBuilder.Create("Media")
                .PrimaryColumn("DiskId", DataType.Short, Constraint.Null)
                .Column("LastSequence", DataType.Short)
                .Column("DiskPrompt")
                .Column("Cabinet")
                .Column("VolumeLabel")
                .Column("Source")
                .Build(),

            TableBuilder.Create("Property")
                .PrimaryColumn("Property")
                .Column("Value")
                .Build(),

            TableBuilder.Create("Shortcut")
                .PrimaryColumn("Shortcut")
                .Column("Directory_")
                .Column("Name")
                .Column("Component_")
                .Column("Target")
                .Column("Arguments")
                .Column("Description")
                .Column("Hotkey", DataType.Short)
                .Column("Icon_")
                .Column("IconIndex", DataType.Short)
                .Column("ShowCmd", DataType.Short)
                .Column("WkDir")
                .Column("DisplayResourceDLL")
                .Column("DisplayResourceId", DataType.Long)
                .Column("DescriptionResourceDLL")
                .Column("DescriptionResourceId", DataType.Long)
                .Build(),

            TableBuilder.Create("Upgrade")
                .PrimaryColumn("UpgradeCode")
                .PrimaryColumn("VersionMin", DataType.Char, Constraint.Null)
                .PrimaryColumn("VersionMax", DataType.Char, Constraint.Null)
                .PrimaryColumn("Language", DataType.Char, Constraint.Null)
                .PrimaryColumn("Attributes", DataType.Long)
                .Column("Remove")
                .Column("ActionProperty", DataType.Char, Constraint.NotNull)
                .Build(),

            TableBuilder.Create("_Validation")
                .PrimaryColumn("Table")
                .PrimaryColumn("Column")
                .Column("Nullable", DataType.Char, Constraint.NotNull)
                .Column("MinValue", DataType.Long)
                .Column("MaxValue", DataType.Long)
                .Column("KeyTable")
                .Column("KeyColumn", DataType.Short)
                .Column("Category")
                .Column("Set")
                .Column("Description")
                .Build()
        };
    }
}