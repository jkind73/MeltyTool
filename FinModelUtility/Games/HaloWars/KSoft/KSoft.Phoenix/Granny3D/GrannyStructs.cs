using System;
using System.Runtime.InteropServices;
using Vector3 = System.Numerics.Vector3;
using Vector4 = System.Numerics.Vector4;
using granny_matrix_4x4 = System.Numerics.Matrix4x4;

namespace KSoft.Granny3D
{
	[StructLayout(LayoutKind.Sequential, Pack=Granny2Dll.K_ASSUMED_POINTER_SIZE)]
	public struct GrannyDataTypeDefinition
	{
		public GrannyMemberType MemberType;
		public CharPtr Name;
		public IntPtr/*TPtr<granny_data_type_definition>*/ ReferenceTypeInternal;
		public int ArrayWidth;
		public int Extra0;
		public int Extra1;
		public int Extra2;
		IntPtr Ignored;

		// #64BIT: Workaround encountered issues trying to define a field which was a TPtr of the same parent type
		public Ptr<GrannyDataTypeDefinition> ReferenceType { get { return new Ptr<GrannyDataTypeDefinition>(this.ReferenceTypeInternal); } }
	};

	[StructLayout(LayoutKind.Sequential, Pack=Granny2Dll.K_ASSUMED_POINTER_SIZE)]
	public struct GrannyVariant
	{
		public Ptr<GrannyDataTypeDefinition> Type;
		public IntPtr Object;
	};

	[StructLayout(LayoutKind.Sequential)]
	public struct GrannyTransform
	{
		public GrannyTransformFlags Flags;
		public Vector3 Position;
		public Vector4 Orientation;
		public Vector3 ScaleShear0;
		public Vector3 ScaleShear1;
		public Vector3 ScaleShear2;
	};

	[StructLayout(LayoutKind.Sequential)]
	public struct GrannyMatrix3X3
	{
		public Vector3 Row0;
		public Vector3 Row1;
		public Vector3 Row2;
	};


	[StructLayout(LayoutKind.Sequential, Pack=Granny2Dll.K_ASSUMED_POINTER_SIZE)]
	public struct GrannyFileInfo
	{
		public Ptr<GrannyArtToolInfo> ArtToolInfo;
		public IntPtr /*granny_exporter_info*/ ExporterInfo;
		public CharPtr FromFileName;
		public ArrayOfRefsPtr<GrannyTexture> Textures;
		public ArrayOfRefsPtr<GrannyMaterial> Materials;
		public ArrayOfRefsPtr<GrannySkeleton> Skeletons;
		public ArrayOfRefsPtr<GrannyVertexData> VertexDatas;
		public ArrayOfRefsPtr<GrannyTriTopology> TriTopologies;
		public ArrayOfRefsPtr<GrannyMesh> Meshes;
		public ArrayOfRefsPtr<GrannyModel> Models;
		public ArrayOfRefsPtr<GrannyTrackGroup> TrackGroups;
		public ArrayOfRefsPtr<GrannyAnimation> Animations;
		public GrannyVariant ExtendedData;
	};

	[StructLayout(LayoutKind.Sequential, Pack=Granny2Dll.K_ASSUMED_POINTER_SIZE)]
	public struct GrannyArtToolInfo
	{
		public CharPtr FromArtToolName;
		public int ArtToolMajorRevision;
		public int ArtToolMinorRevision;
		public float UnitsPerMeter;
		public Vector3 Origin;
		public Vector3 RightVector;
		public Vector3 UpVector;
		public Vector3 BackVector;
		public GrannyVariant ExtendedData;
	};

	#region granny_texture
	[StructLayout(LayoutKind.Sequential, Pack=Granny2Dll.K_ASSUMED_POINTER_SIZE)]
	public struct GrannyTexture
	{
		public CharPtr FromFileName;
		public GrannyTextureType TextureType;
		public int Width;
		public int Height;
		public GrannyTextureEncoding Encoding;
		public int SubFormat;
		public Ptr<GrannyPixelLayout> Layout;
		public ArrayPtr<GrannyTextureImage> Images;
		public GrannyVariant ExtendedData;
	};

	[StructLayout(LayoutKind.Sequential)]
	public struct GrannyPixelLayout;

	[StructLayout(LayoutKind.Sequential)]
	public struct GrannyTextureImage;
	#endregion

	#region granny_material
	[StructLayout(LayoutKind.Sequential, Pack=Granny2Dll.K_ASSUMED_POINTER_SIZE)]
	public struct GrannyMaterial
	{
		public CharPtr Name;
		public ArrayPtr<GrannyMaterialMap> Maps;
		public Ptr<GrannyTexture> Texture;
		public GrannyVariant ExtendedData;
	};

	[StructLayout(LayoutKind.Sequential)]
	public struct GrannyMaterialMap;
	#endregion

	#region granny_skeleton
	[StructLayout(LayoutKind.Sequential, Pack=Granny2Dll.K_ASSUMED_POINTER_SIZE)]
	public struct GrannySkeleton
	{
		public CharPtr Name;
		public ArrayPtr<GrannyBone> Bones;
		public int LODType;
	};

	[StructLayout(LayoutKind.Sequential, Pack=Granny2Dll.K_ASSUMED_POINTER_SIZE)]
	public struct GrannyBone
	{
		public CharPtr Name;
		public int ParentIndex;
		public GrannyTransform LocalTransform;
		public granny_matrix_4x4 InverseWorld4x4;
		public float LODError;
		public GrannyVariant ExtendedData;
	};
	#endregion

	#region granny_mesh
	[StructLayout(LayoutKind.Sequential, Pack=Granny2Dll.K_ASSUMED_POINTER_SIZE)]
	public struct GrannyMesh
	{
		public CharPtr Name;
		public Ptr<GrannyVertexData> PrimaryVertexData;
		public ArrayPtr<GrannyMorphTarget> MorphTargets;
		public Ptr<GrannyTriTopology> PrimaryTopology;
		public ArrayPtr<GrannyMaterialBinding> MaterialBindings;
		public ArrayPtr<GrannyBoneBinding> BoneBindings;
		public GrannyVariant ExtendedData;
	};

	[StructLayout(LayoutKind.Sequential, Pack=Granny2Dll.K_ASSUMED_POINTER_SIZE)]
	public struct GrannyVertexData
	{
		public Ptr<GrannyDataTypeDefinition> VertexType;
		public ArrayPtr Vertices;
		public ArrayCharPtr VertexComponentNames;
		public ArrayPtr<GrannyVertexAnnotationSet> VertexAnnotationSets;
	};
	[StructLayout(LayoutKind.Sequential)]
	public struct GrannyVertexAnnotationSet;

	[StructLayout(LayoutKind.Sequential)]
	public struct GrannyMorphTarget;

	[StructLayout(LayoutKind.Sequential, Pack=Granny2Dll.K_ASSUMED_POINTER_SIZE)]
	public struct GrannyTriTopology
	{
		public ArrayPtr<GrannyTriMaterialGroup> Groups;
		public ArrayPtr Indices;
		public ArrayPtr<ushort> Indices16;
		public ArrayPtr VertexToVertexMap;
		public ArrayPtr VertexToTriangleMap;
		public ArrayPtr SideToNeightborMap;
		public ArrayPtr BonesForTriangle;
		public ArrayPtr TriangleToBoneIndices;
		public ArrayPtr<GrannyTriAnnotationSet> TriAnnotationSets;
	};
	[StructLayout(LayoutKind.Sequential)]
	public struct GrannyTriMaterialGroup
	{
		public int MaterialIndex;
		public int TriFirst;
		public int TriCount;
	};
	[StructLayout(LayoutKind.Sequential, Pack=Granny2Dll.K_ASSUMED_POINTER_SIZE)]
	public struct GrannyTriAnnotationSet
	{
		public CharPtr Name;
		public Ptr<GrannyDataTypeDefinition> TriAnnotationType;
		public ArrayPtr TriAnnotations;
		public int IndicesMapFromTriToAnnotation; // BOOL
		public ArrayPtr<int> TriAnnotationIndices;
	};

	[StructLayout(LayoutKind.Sequential, Pack=Granny2Dll.K_ASSUMED_POINTER_SIZE)]
	public struct GrannyMaterialBinding
	{
		public Ptr<GrannyMaterial> Material;
	};

	[StructLayout(LayoutKind.Sequential, Pack=Granny2Dll.K_ASSUMED_POINTER_SIZE)]
	public struct GrannyBoneBinding
	{
		public CharPtr BoneName;
		public Vector3 OBBMin;
		public Vector3 OBBMax;
		public ArrayPtr<int> TriangleIndices;
	};
	#endregion

	#region granny_model
	[StructLayout(LayoutKind.Sequential, Pack=Granny2Dll.K_ASSUMED_POINTER_SIZE)]
	public struct GrannyModel
	{
		public CharPtr Name;
		public Ptr<GrannySkeleton> Skeleton;
		public GrannyTransform InitialPlacement;
		public ArrayPtr<GrannyModelMeshBinding> MeshBindings;
	};

	[StructLayout(LayoutKind.Sequential, Pack=Granny2Dll.K_ASSUMED_POINTER_SIZE)]
	public struct GrannyModelMeshBinding
	{
		public Ptr<GrannyMesh> Mesh;
	};
	#endregion

	#region granny_track_group
	[StructLayout(LayoutKind.Sequential, Pack=Granny2Dll.K_ASSUMED_POINTER_SIZE)]
	public struct GrannyTrackGroup
	{
		public CharPtr Name;
		public ArrayPtr /*granny_vector_track */ VectorTracks;
		public ArrayPtr /*granny_transform_track */ TransformTracks;
		public ArrayPtr<float> TransformLODErrors;
		public ArrayPtr /*granny_text_track */ TextTracks;
		public GrannyTransform InitialPlacement;
		public GrannyTrackGroupFlags Flags;
		public Vector3 LoopTranslation;
		public IntPtr /*granny_periodic_loop */ PeriodicLoop;
		public IntPtr /*granny_transform_track */ RootMotion;
		public GrannyVariant ExtendedData;
	};
	#endregion

	#region granny_animation
	[StructLayout(LayoutKind.Sequential, Pack=Granny2Dll.K_ASSUMED_POINTER_SIZE)]
	public struct GrannyAnimation
	{
		public CharPtr Name;
		public float Duration;
		public float TimeStep;
		public float Oversampling;
		public ArrayOfRefsPtr<GrannyTrackGroup> TrackGroups;
	};
	#endregion

	[StructLayout(LayoutKind.Sequential)]
	public struct GrannyModelInstance;

	[StructLayout(LayoutKind.Sequential)]
	public struct GrannyControl;

	[StructLayout(LayoutKind.Sequential)]
	public struct GrannyModelControlBinding;

	[StructLayout(LayoutKind.Sequential)]
	public struct GrannyWorldPose;

	[StructLayout(LayoutKind.Sequential)]
	public struct GrannyMeshBinding;

	[StructLayout(LayoutKind.Sequential)]
	public struct GrannyMeshDeformer;

	[StructLayout(LayoutKind.Sequential)]
	public struct GrannyLocalPose;
}
