using System;
using System.Runtime.InteropServices;
using Vector3 = System.Numerics.Vector3;
using granny_matrix_4x4 = System.Numerics.Matrix4x4;

namespace KSoft.Granny3D
{
	public static class Granny2Dll
	{
		public const int K_ASSUMED_POINTER_SIZE = sizeof(ulong);

		const string K_DLL_NAME_ = @"Granny3D\Granny2.dll";
		const CharSet K_CHAR_SET_ = CharSet.Ansi;

		[DllImport(K_DLL_NAME_, CharSet=K_CHAR_SET_)]
		public static extern bool GrannyFindBoneByName(Ptr<GrannySkeleton> skeleton, string boneName, out int boneIndex);

		[DllImport(K_DLL_NAME_)]
		public static extern void GrannyBuildCompositeTransform4x4(
			[In] ref GrannyTransform transform,
			out granny_matrix_4x4 matrix4X4);

		[DllImport(K_DLL_NAME_)]
		public static extern Ptr<GrannyTexture> GrannyGetMaterialTextureByType(
			Ptr<GrannyMaterial> material,
			GrannyMaterialTextureType channel);

		[DllImport(K_DLL_NAME_)]
		public static extern void GrannyConvertSingleObject(
			Ptr<GrannyDataTypeDefinition> sourceType,
			IntPtr sourceObject,
			Ptr<GrannyDataTypeDefinition> destType,
			IntPtr destObject);

		[DllImport(K_DLL_NAME_)]
		public static extern int GrannyGetTypeTableCount(IntPtr /* granny_data_type_definition** */ typeTable);

		[DllImport(K_DLL_NAME_)]
		public static extern void GrannyBuildSkeletonRelativeTransforms(
			int sourceTransformStride,
			[In] GrannyTransform[] sourceTransforms,
			int sourceParentStride,
			[In] int[] sourceParents,
			int count,
			int resultStride,
			[Out] GrannyTransform[] results);

		#region granny_file
		[DllImport(K_DLL_NAME_)]
		public static extern IntPtr /* granny_file* */ GrannyReadEntireFile(string filename);
		[DllImport(K_DLL_NAME_)]
		public static extern void GrannyFreeFile(IntPtr /* granny_file* */ file);
		[DllImport(K_DLL_NAME_)]
		public static extern void GrannyFreeFileSection(IntPtr /* granny_file* */ file, int sectionIndex);
		[DllImport(K_DLL_NAME_)]
		public static extern Ptr<GrannyFileInfo> GrannyGetFileInfo(IntPtr /* granny_file* */file);
		#endregion

		#region granny_file_info
		[DllImport(K_DLL_NAME_)]
		public static extern bool GrannyComputeBasisConversion(
			[In] Ptr<GrannyFileInfo> fileInfo,
			float desiredUnitsPerMeter,
			[In] ref Vector3 desiredOrigin3,
			[In] ref Vector3 desiredRight3,
			[In] ref Vector3 desiredUp3,
			[In] ref Vector3 desiredBack3,
			out Vector3 resultAffine3,
			out GrannyMatrix3X3 resultLinear3X3,
			out GrannyMatrix3X3 resultInverseLinear3X3);

		[DllImport(K_DLL_NAME_)]
		public static extern void GrannyTransformFile(
			Ptr<GrannyFileInfo> fileInfo,
			[In] ref Vector3 affine3,
			[In] ref GrannyMatrix3X3 linear3X3,
			[In] ref GrannyMatrix3X3 inverseLinear3X3,
			float affineTolerance,
			float linearTolerance,
			GrannyTransformFileFlags flags);
		#endregion

		#region granny_mesh
		[DllImport(K_DLL_NAME_)]
		public static extern Ptr<GrannyTriMaterialGroup> GrannyGetMeshTriangleGroups(Ptr<GrannyMesh> mesh);
		[DllImport(K_DLL_NAME_)]
		public static extern bool GrannyMeshIsRigid(Ptr<GrannyMesh> mesh);
		[DllImport(K_DLL_NAME_)]
		public static extern int GrannyGetMeshIndexCount(Ptr<GrannyMesh> mesh);
		[DllImport(K_DLL_NAME_)]
		public static extern void GrannyCopyMeshIndices(Ptr<GrannyMesh> mesh, int bytesPerIndex, IntPtr dstIndexData);
		[DllImport(K_DLL_NAME_)]
		public static extern Ptr<GrannyDataTypeDefinition> GrannyGetMeshVertexType(Ptr<GrannyMesh> mesh);
		[DllImport(K_DLL_NAME_)]
		public static extern int GrannyGetMeshVertexCount(Ptr<GrannyMesh> mesh);
		[DllImport(K_DLL_NAME_)]
		public static extern IntPtr GrannyGetMeshVertices(Ptr<GrannyMesh> mesh);
		[DllImport(K_DLL_NAME_)]
		public static extern void GrannyCopyMeshVertices(Ptr<GrannyMesh> mesh, Ptr<GrannyDataTypeDefinition> vertType, IntPtr dstVertexData);
		[DllImport(K_DLL_NAME_)]
		public static extern int GrannyGetMeshTriangleCount(Ptr<GrannyMesh> mesh);
		[DllImport(K_DLL_NAME_)]
		public static extern int GrannyGetMeshTriangleGroupCount(Ptr<GrannyMesh> mesh);
		#endregion

		#region granny_model_instance
		[DllImport(K_DLL_NAME_)]
		public static extern Ptr<GrannyModelInstance> GrannyInstantiateModel(Ptr<GrannyModel> model);
		[DllImport(K_DLL_NAME_)]
		public static extern void GrannyFreeModelInstance(Ptr<GrannyModelInstance> modelInstance);
		[DllImport(K_DLL_NAME_)]
		public static extern Ptr<GrannyModelControlBinding> GrannyModelControlsBegin(Ptr<GrannyModelInstance> model);
		[DllImport(K_DLL_NAME_)]
		public static extern Ptr<GrannyModelControlBinding> GrannyModelControlsEnd(Ptr<GrannyModelInstance> model);
		[DllImport(K_DLL_NAME_)]
		public static extern Ptr<GrannyModelControlBinding> GrannyModelControlsNext(Ptr<GrannyModelControlBinding> binding);

		[DllImport(K_DLL_NAME_)]
		public static extern Ptr<GrannySkeleton> GrannyGetSourceSkeleton(Ptr<GrannyModelInstance> model);

		[DllImport(K_DLL_NAME_)]
		public static extern void GrannySetModelClock(Ptr<GrannyModelInstance> modelInstance, float newClock);

		[DllImport(K_DLL_NAME_)]
		public static extern void GrannyUpdateModelMatrix(
			Ptr<GrannyModelInstance> modelInstance,
			float secondsElapsed,
			[In] ref granny_matrix_4x4 modelMatrix4X4,
			out granny_matrix_4x4 destMatrix4X4,
			bool inverse);

		[DllImport(K_DLL_NAME_)]
		public static extern void GrannySampleModelAnimations(
			Ptr<GrannyModelInstance> modelInstance,
			int firstBone,
			int boneCount,
			Ptr<GrannyLocalPose> result);

		[DllImport(K_DLL_NAME_)]
		public static extern void GrannyFreeCompletedModelControls(Ptr<GrannyModelInstance> modelInstance);
		#endregion

		#region granny_control
		[DllImport(K_DLL_NAME_)]
		public static extern Ptr<GrannyControl> GrannyPlayControlledAnimation(
			float startTime,
			Ptr<GrannyAnimation> animation,
			Ptr<GrannyModelInstance> model);
		[DllImport(K_DLL_NAME_)]
		public static extern void GrannyFreeControl(Ptr<GrannyControl> control);
		[DllImport(K_DLL_NAME_)]
		public static extern void GrannySetControlLoopCount(Ptr<GrannyControl> control, int loopCount);
		[DllImport(K_DLL_NAME_)]
		public static extern void GrannyFreeControlOnceUnused(Ptr<GrannyControl> control);
		[DllImport(K_DLL_NAME_)]
		public static extern Ptr<GrannyControl> GrannyGetControlFromBinding(Ptr<GrannyModelControlBinding> binding);
		[DllImport(K_DLL_NAME_)]
		public static extern float GrannyEaseControlOut(Ptr<GrannyControl> control, float duration);
		[DllImport(K_DLL_NAME_)]
		public static extern void GrannyCompleteControlAt(Ptr<GrannyControl> control, float atSeconds);
		[DllImport(K_DLL_NAME_)]
		public static extern IntPtr /* void** */ GrannyGetControlUserDataArray(Ptr<GrannyControl> control);
		#endregion

		#region granny_world_pose
		[DllImport(K_DLL_NAME_)]
		public static extern Ptr<GrannyWorldPose> GrannyNewWorldPose(int boneCount);
		[DllImport(K_DLL_NAME_)]
		public static extern void GrannyFreeWorldPose(Ptr<GrannyWorldPose> worldPose);
		[DllImport(K_DLL_NAME_)]
		public static extern Ptr<granny_matrix_4x4> GrannyGetWorldPoseComposite4x4Array(Ptr<GrannyWorldPose> worldPose);
		[DllImport(K_DLL_NAME_)]
		public static extern void GrannyBuildWorldPose(
			Ptr<GrannySkeleton> skeleton,
			int firstBone,
			int boneCount,
			Ptr<GrannyLocalPose> localPose,
			[In] ref granny_matrix_4x4 offset4X4,
			Ptr<GrannyWorldPose> result);
		[DllImport(K_DLL_NAME_)]
		public static extern Ptr<granny_matrix_4x4> GrannyGetWorldPose4x4(Ptr<GrannyWorldPose> worldPose, int boneIndex);
		#endregion

		#region granny_local_pose
		[DllImport(K_DLL_NAME_)]
		public static extern Ptr<GrannyLocalPose> GrannyNewLocalPose(int boneCount);
		[DllImport(K_DLL_NAME_)]
		public static extern void GrannyFreeLocalPose(Ptr<GrannyLocalPose> pose);
		[DllImport(K_DLL_NAME_)]
		public static extern void GrannyGetWorldMatrixFromLocalPose(
			[In] Ptr<GrannySkeleton> skeleton,
			int boneIndex,
			[In] Ptr<GrannyLocalPose> localPose,
			[In] ref granny_matrix_4x4 offset4X4,
			out granny_matrix_4x4 result4X4,
			[In] int[] sparseBoneArray,
			[In] int[] sparseBoneArrayReverse);
		#endregion

		#region granny_mesh_binding
		[DllImport(K_DLL_NAME_)]
		public static extern Ptr<GrannyMeshBinding> GrannyNewMeshBinding(
			Ptr<GrannyMesh> mesh,
			Ptr<GrannySkeleton> fromSkeleton,
			Ptr<GrannySkeleton> toSkeleton);
		[DllImport(K_DLL_NAME_)]
		public static extern void GrannyFreeMeshBinding(Ptr<GrannyMeshBinding> binding);
		[DllImport(K_DLL_NAME_)]
		public static extern Ptr<int> GrannyGetMeshBindingToBoneIndices(Ptr<GrannyMeshBinding> binding);
		#endregion

		#region granny_mesh_deformer
		[DllImport(K_DLL_NAME_)]
		public static extern Ptr<GrannyMeshDeformer> GrannyNewMeshDeformer(
			Ptr<GrannyDataTypeDefinition> inputVertexLayout,
			Ptr<GrannyDataTypeDefinition> outputVertexLayout,
			GrannyDeformationType deformationType,
			GrannyDeformerTailFlags tailFlag);
		[DllImport(K_DLL_NAME_)]
		public static extern void GrannyFreeMeshDeformer(Ptr<GrannyMeshDeformer> deformer);
		[DllImport(K_DLL_NAME_)]
		public static extern void GrannyDeformVertices(
			Ptr<GrannyMeshDeformer> deformer,
			[In] int[] matrixIndices,
			[In] ref granny_matrix_4x4 matrixBuffer4X4,
			int vertexCount,
			IntPtr sourceVertices,
			IntPtr destVertices);
		#endregion
	};
}
