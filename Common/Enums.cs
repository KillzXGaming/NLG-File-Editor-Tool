using System;
using System.Collections.Generic;
using System.Text;

namespace NextLevelLibrary
{
    public enum HitboxShapeType
    {
        BOX = 0,
        SPHERE,
        CAPSULE,
        CONE,
        CYLINDER,
    }

    public enum COGLightType
    {
        ScreenPlane = 0,
        PointLight = 2, //Default type if not set
        SpotLight = 3,
        TubeLight = 4,
    }

    public enum VertexDataFormat
    {
        Float16,
        Float32,
        Float32_32,
        Float32_32_32,
    }

    public enum ResourceType : byte
    {
        NONE = 0,
        TABLE = 1,
        DATA = 2,
    }

    public enum IndexFormat : ushort
    {
        Index_16 = 0x0,
        Index_8 = 0x8000,
    }

    public enum FileIdentifiers : uint
    {
        ModelFile = 0x30DB9D05,
        TextureFile = 0x50D377E9,
        AnimationFile = 0x24FA2400,
    }

    public enum BlockType : byte
    {
        FileTable = 0x4,
        FileData = 0x8,
    }

    public enum ChunkFileType : ushort
    {
        FileTable = 0x1,
        TextureBundles = 0x20,
        ModelBundles = 0x21,
        AnimationBundles = 0x1302,
        Room = 16,
        CutsceneNLB = 0x30,
        Config = 0x31, //Text based parameters
        Video = 0x1200, //MPEG Video File
        AudioBanks = 0x3000,
        Effects = 0x4000,
        Script = 0x5000,
        GameObjectScriptTable = 0x6500,
        GameObject = 0x6510,
        MaterialEffects = 0xB300,
        Texture = 0xB500,
        Shaders = 0xB400,
        ShaderConstants = 0xB404,
        MaterialParams = 0xB310,
        MaterialShaders = 0xB320,
        Model = 0xB000,
        CollisionStatic = 0xC107,
        Hitboxes = 0xC300,
        HitboxRigged = 0xD000,
        ClothPhysics = 0xE000,
        AnimationData = 0x7000,
        Font = 0x7010,
        MessageData = 0x7020,
        Skeleton = 0x7100,
        VAND = 0x9501,
    }

    public enum ChunkDataType : ushort
    {
        CutsceneNLB = 0x1200,

        AudioData1 = 0xA251,
        AudioData2 = 0xA252,
        AudioData3 = 0xA253,
        AudioData4 = 0xA254,

       // MaterialName = 0xB333,

        ShaderA = 0xB401,
        ShaderB = 0xB402,

        TextureHeader = 0xB501,
        TextureData = 0xB502,

        CollisionDataStart = 0xC100,
        CollisionHeader = 0xC101,
        CollisionSearch = 0xC102,
        CollisionSearchTriIndices = 0xC103,
        CollisionVertexPositions = 0xC110,
        CollisionTriIndices = 0xC111,
        CollisionTriNormals = 0xC112,
        CollisionTriNormalIndices = 0xC113,
        CollisionMaterialHashes = 0xC114,
        CollisionTriMaterialIndices = 0xC115,
        CollisionTriPropertyIndices = 0xC116,

        ModelTransform = 0xB001, //Matrix4x4.
        ModelInfo = 0xB002, //Contains mesh count and model hash
        MeshInfo = 0xB003,
        VertexStartPointers = 0xB004,
        MeshBuffers = 0xB005, //Vertex and index buffer
        MaterialData = 0xB006,
        MaterialLookupTable = 0xB007,
        BoundingRadius = 0xB008,
        BoundingBox = 0xB009,
        MeshMorphInfos = 0xB00A,
        MeshMorphIndexBuffer = 0xB00B,
        ModelUnknownSection = 0xB00C,

        FontData = 0x7011,
        MessageData = 0x7020,
        ShaderData = 0xB400,
        UILayoutStart = 0x7000,
        UILayoutHeader = 0x7001,
        UILayoutData = 0x7002, //Without header
        UILayout = 0x7003, //All parts combined

        HavokPhysics = 0xC900,
        PhysicData2 = 0xC901,
        HitboxObjects = 0xC301,
        HitboxObjectParams = 0xC302,

        HitboxRiggedHeader = 0xD001, 
        HitboxRiggedData = 0xD002,

        SkinControllerStart = 0xB100,
        SkinBindingModelAssign = 0xB101, //Contains just the model hash to assign to
        SkinMatrices = 0xB102, //Matrices for skinning
        SkinHashes = 0xB103, //Hashes for remapping bone indices

        //Scripts handle various things.
        //Lighting, NIS cutscene triggers, object placements.
        ScriptHashBundle = 0x5011, //Bundles for multiple hash lists,
        ScriptData = 0x5012, //Raw script data + op codes to execute,
        ScriptHeader = 0x5013, //Some sort of 8 byte header used per script. Includes hash table size to allocate
        ScriptFunctionTable = 0x5014, //Function lists by hash. They index where to execute code
        ScriptStringHashes = 0x5015, //A list of hashes, often used if strings are present in ScriptData

        //These are used for COGScriptInterpreter types
        //COG = component
        GameObjectDB = 0x6500,
        GameObjectDBScriptHashTable = 0x6501, //Determines the functions to excute for game object
        GameObjectDBHashScriptIndexTable = 0x6502,
        GameObjectDBScriptHash = 0x6503, //Name hash
        GameObjectScriptHash = 0x6511, //The script type.
        GameObjectComponentOffsets = 0x6512, //List of property offsets, which data is set in the script
        GameObjectComponentHashes = 0x6513, //List of property hashes
        GameObjectComponentList = 0x6514, //List of properties with hash + offset + unknown value
        GameObjectParentHash = 0x6515, //Determines the class to inherit. If used, no properties will be present in file.

        SkeletonHeader = 0x7101,
        SkeletonBoneInfo = 0x7102,
        SkeletonBoneTransform = 0x7103,
        SkeletonBoneIndexList = 0x7104,
        SkeletonBoneHashList = 0x7105,
        SkeletonBoneParenting = 0x7106,

        //

        MaterialRasterizerConfig = 0xB321,
        MaterialDepthConfig = 0xB322,
        MaterialBlendConfig = 0xB323,

        MaterialShaderHeader = 0xB325,
        MaterialShaderName = 0xB326,
        MaterialParameterIndices = 0xB327,
        MaterialParameterOffsets = 0xB328,

        MaterialShaderAttrLocations = 0xB329,
        MaterialShaderAttrLocationOffsets = 0xB32A,

        MaterialShaderProgramLocations = 0xB32B,
        MaterialShaderProgramOffsets = 0xB32D,
        MaterialShaderUnknown = 0xB32E,

        MaterialVariation = 0xB330,
        ShaderProgramRenderParams = 0xB331,
        ShaderProgramHeader = 0xB332,
        ShaderProgramLocationOffsets = 0xB333,
        ShaderProgramLocIndices = 0xB334,
        ShaderProgramLocFlags = 0xB335,
        ShaderProgramHashes = 0xB337,
    }
}