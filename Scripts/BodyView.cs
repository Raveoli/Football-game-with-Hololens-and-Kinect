/*
 * BodyView.cs
 *
 * Displays spheres for Kinect body joints
 * Requires the BodyDataConverter script or the BodyDataReceiver script
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum HNJOINT
{
    HIP_CENTER = 0,
    SPINE,
    SHOULDER_CENTER,
    HEAD,
    SHOULDER_LEFT,
    ELBOW_LEFT,
    WRIST_LEFT,
    HAND_LEFT,
    SHOULDER_RIGHT,
    ELBOW_RIGHT,
    WRIST_RIGHT,
    HAND_RIGHT,
    HIP_LEFT,
    KNEE_LEFT,
    ANKLE_LEFT,
    FOOT_LEFT,
    HIP_RIGHT,
    KNEE_RIGHT,
    ANKLE_RIGHT,
    FOOT_RIGHT,
    //KINECT V2 NEW JOINTS
    SPINE_SHOULDER,
    HANDTIP_LEFT,
    THUMB_LEFT,
    HANDTIP_RIGHT,
    THUMB_RIGHT,
    JOINT_COUNT
};

public enum HNJOINTSTATE
{
    HNJOINTSTATE_NOTTRACKED = 0,
    HNJOINTSTATE_INFERRED,
    HNJOINTSTATE_TRACKED
};

public enum HNHANDSTATE
{
    HNHANDSTATE_UNKNOWN = 0,
    HNHANDSTATE_NOTTRACKED = 1,
    HNHANDSTATE_OPEN = 2,
    HNHANDSTATE_CLOSED = 3,
    HNHANDSTATE_LASSO = 4
};

public class HNSkeletonBone
{
    public HNJOINT from, to, sizeTo;
    public int index, sizeFrom;
    public bool isTerminalBone = false, isJointDistanceDependentSize = true, useCapsuleCollider = true;
    public Vector3 sizeFactor;
    public float radius;
}

public class BodyView : MonoBehaviour {

    public GameObject BodySourceManager;

    // Dictionary relating tracking IDs to displayed GameObjects
    private Dictionary<long, GameObject> _Bodies = new Dictionary<long, GameObject>();
    private BodyDataReceiver _BodyDataReceiver;
    public static List<HNSkeletonBone> Bones = new List<HNSkeletonBone>();
    protected List<HNSkeletonBone> _bones;
    protected UnityEngine.Object _resource;
    protected GameObject[] _boneObjects;
    public PhysicMaterial _boneMaterial;
    public System.String _prefixName;
    private Vector3[] joints;

    void Start()
    {
        _resource = Resources.Load("Prefabs/Bone", typeof(GameObject));
        assignBones();
        _bones = getBones();
        makeSkeleton();
    }

    void Update() {

        if (BodySourceManager == null) {
            return;
        }

        // Dictionary of tracked bodies from the Kinect or from data
        // sent over the server
        Dictionary<long, Vector3[]> bodies;

        // Is the body data coming from the BodyDataReceriver script?
        _BodyDataReceiver = BodySourceManager.GetComponent<BodyDataReceiver>();
        if (_BodyDataReceiver == null) {
            return;
        } else {
            bodies = _BodyDataReceiver.GetData();
        }

        if (bodies == null) {
            return;
        }

        // Delete untracked bodies
        List<long> trackedIDs = new List<long>(bodies.Keys);
        List<long> knownIDs = new List<long>(_Bodies.Keys);
        foreach (long trackingID in knownIDs) {

            if (!trackedIDs.Contains(trackingID)) {
                Destroy(_Bodies[trackingID]);
                _Bodies.Remove(trackingID);
            }
        }

        // Add and update tracked bodies
        foreach (long trackingID in bodies.Keys) {

            // Add tracked bodies if they are not already being displayed
            if (!_Bodies.ContainsKey(trackingID)) {
                _Bodies[trackingID] = CreateBodyObject(trackingID);
                joints = bodies[trackingID];
            }

            // Update the positions of each body's joints
            RefreshBodyObject(bodies[trackingID], _Bodies[trackingID]);
            joints = bodies[trackingID];
            updateSkeleton();
        }
    }

    // Create a GameObject given a tracking ID
    private GameObject CreateBodyObject(long id) {

        GameObject body = new GameObject("Body:" + id);

        for (int i = 0; i < 25; i++) {
            GameObject jointObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);

            jointObj.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
			jointObj.name = _prefixName+i.ToString();
            jointObj.transform.parent = body.transform;
			jointObj.tag = "Targets";
        }

        return body;
    }

    // Update the joint GameObjects of a given body
    private void RefreshBodyObject(Vector3[] jointPositions, GameObject bodyObj) {

        for (int i = 0; i < 25; i++) {
            Vector3 jointPos = jointPositions[i];

			Transform jointObj = bodyObj.transform.FindChild(_prefixName+i.ToString());
            jointObj.localPosition = jointPos;
        }
    }

    protected virtual void makeSkeleton()
    {
        _boneObjects = new GameObject[_bones.Count];
        for (int i = 0; i < _bones.Count; i++)
        {
            _boneObjects[i] = makeSkeletonBone(_bones[i]);
        }
    }

    protected virtual GameObject makeSkeletonBone(HNSkeletonBone bone)
    {
        GameObject boneObject = Instantiate(_resource, Vector3.zero, Quaternion.identity) as GameObject;
        Rigidbody Rb = (Rigidbody)boneObject.AddComponent<Rigidbody>();
        Rb.isKinematic = true;
        Rb.useGravity = false;
        Rb.interpolation = RigidbodyInterpolation.None;
        Rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        makeCollider(boneObject, bone);
        boneObject.transform.parent = gameObject.transform;
        boneObject.name = _prefixName + bone.index;
        boneObject.tag = "Targets";
        return boneObject;
    }

    protected virtual void makeCollider(GameObject boneObject, HNSkeletonBone bone)
    {
        if (bone.useCapsuleCollider)
            makeCapsuleCollider(boneObject, bone);
        else
            makeBoxCollider(boneObject);
    }

    protected virtual void makeCapsuleCollider(GameObject boneObject, HNSkeletonBone bone)
    {
        CapsuleCollider col = (CapsuleCollider)boneObject.AddComponent<CapsuleCollider>();
        col.center = new Vector3(0f, 0f, 0.5f);
        col.direction = 2;
        col.height = 1f;
        col.radius = bone.radius;
        col.material = _boneMaterial;
        col.isTrigger = false;
    }

    protected virtual void makeBoxCollider(GameObject boneObject)
    {
        BoxCollider col = (BoxCollider)boneObject.AddComponent<BoxCollider>();
        col.size = Vector3.one;
        col.material = _boneMaterial;
        col.isTrigger = false;
    }

    protected virtual void updateSkeleton()
    {
        Vector3[] boneSizes = calculateBoneSizes(joints);
        for (int i = 0; i < _bones.Count; i++)
        {
            updateBone(_boneObjects[i], joints[(int)_bones[i].from], joints[(int)_bones[i].to], boneSizes[i]);
        }
    }

    protected virtual void updateBone(GameObject boneObject, Vector3 fromVector, Vector3 toVector, Vector3 boneSize)
    {
        boneObject.GetComponent<MeshRenderer>().enabled = true;
        boneObject.transform.localScale = boneSize;
        Rigidbody Rb = boneObject.GetComponent<Rigidbody>();

        Vector3 position = Rb.transform.position;
        Rb.transform.localPosition = fromVector;
        Vector3 tposition = Rb.transform.position;
        Rb.transform.position = position;
        Rb.MovePosition(tposition);

        //		Rb.MovePosition (fromVector);
        //		boneObject.transform.localPosition = fromVector;

        Quaternion temp = Rb.transform.rotation;
        Rb.transform.localRotation = Quaternion.LookRotation(toVector - fromVector);
        Quaternion rotation = Rb.transform.rotation;
        Rb.transform.rotation = temp;
        Rb.MoveRotation(rotation);
    }

    static void assignBones()
    {
        Bones.Clear();
        Bones.Add(new HNSkeletonBone() { from = HNJOINT.SHOULDER_CENTER, to = HNJOINT.HEAD, index = 0, isTerminalBone = true, isJointDistanceDependentSize = true, sizeFactor = new Vector3(0.8f, 1.2f, 1.0f), sizeFrom = (int)HNJOINT.HEAD, sizeTo = HNJOINT.SHOULDER_CENTER, radius = 0.4f });//0
        Bones.Add(new HNSkeletonBone() { from = HNJOINT.SPINE_SHOULDER, to = HNJOINT.SHOULDER_CENTER, index = 1, isJointDistanceDependentSize = false, sizeFactor = new Vector3(0.6f, 0.5f, 1.0f), sizeFrom = 0, radius = 0.4f });//1
        Bones.Add(new HNSkeletonBone() { from = HNJOINT.SPINE, to = HNJOINT.SPINE_SHOULDER, index = 2, isJointDistanceDependentSize = true, sizeFactor = new Vector3(1.3f, 0.8f, 1.0f), sizeFrom = (int)HNJOINT.SHOULDER_RIGHT, sizeTo = HNJOINT.SHOULDER_LEFT, radius = 0.4f, useCapsuleCollider = false });//2
        Bones.Add(new HNSkeletonBone() { from = HNJOINT.HIP_CENTER, to = HNJOINT.SPINE, index = 3, isJointDistanceDependentSize = true, sizeFactor = new Vector3(0.6f, 2.0f, 1.0f), sizeFrom = (int)HNJOINT.HIP_LEFT, sizeTo = HNJOINT.HIP_RIGHT, radius = 0.4f, useCapsuleCollider = false });//3
        Bones.Add(new HNSkeletonBone() { from = HNJOINT.SPINE_SHOULDER, to = HNJOINT.SHOULDER_RIGHT, index = 4, isJointDistanceDependentSize = true, sizeFactor = new Vector3(0.6f, 0.8f, 1.0f), sizeFrom = (int)HNJOINT.SHOULDER_CENTER, sizeTo = HNJOINT.SPINE_SHOULDER, radius = 0.4f, useCapsuleCollider = false });//4
        Bones.Add(new HNSkeletonBone() { from = HNJOINT.SPINE_SHOULDER, to = HNJOINT.SHOULDER_LEFT, index = 5, isJointDistanceDependentSize = true, sizeFactor = new Vector3(0.6f, 0.8f, 1.0f), sizeFrom = (int)HNJOINT.SHOULDER_CENTER, sizeTo = HNJOINT.SPINE_SHOULDER, radius = 0.4f, useCapsuleCollider = false });//5
        Bones.Add(new HNSkeletonBone() { from = HNJOINT.HIP_CENTER, to = HNJOINT.HIP_RIGHT, index = 6, isJointDistanceDependentSize = true, sizeFactor = new Vector3(0.6f, 0.8f, 1.0f), sizeFrom = (int)HNJOINT.HIP_CENTER, sizeTo = HNJOINT.SPINE, radius = 0.4f, useCapsuleCollider = false });//6
        Bones.Add(new HNSkeletonBone() { from = HNJOINT.HIP_CENTER, to = HNJOINT.HIP_LEFT, index = 7, isJointDistanceDependentSize = true, sizeFactor = new Vector3(0.6f, 0.8f, 1.0f), sizeFrom = (int)HNJOINT.HIP_CENTER, sizeTo = HNJOINT.SPINE, radius = 0.4f, useCapsuleCollider = false });//7

        // Right Arm    
        Bones.Add(new HNSkeletonBone() { from = HNJOINT.SHOULDER_RIGHT, to = HNJOINT.ELBOW_RIGHT, index = 8, isJointDistanceDependentSize = true, sizeFactor = new Vector3(0.6f, 0.5f, 1.0f), sizeFrom = (int)HNJOINT.SHOULDER_CENTER, sizeTo = HNJOINT.SHOULDER_LEFT, radius = 0.4f });//8
        Bones.Add(new HNSkeletonBone() { from = HNJOINT.ELBOW_RIGHT, to = HNJOINT.WRIST_RIGHT, index = 9, isJointDistanceDependentSize = false, sizeFactor = new Vector3(0.6f, 0.8f, 1.0f), sizeFrom = 8, radius = 0.4f });//9
        Bones.Add(new HNSkeletonBone() { from = HNJOINT.WRIST_RIGHT, to = HNJOINT.HAND_RIGHT, index = 10, isJointDistanceDependentSize = false, sizeFactor = new Vector3(0.6f, 1.2f, 1.0f), sizeFrom = 9, radius = 0.4f });//10
        Bones.Add(new HNSkeletonBone() { from = HNJOINT.HAND_RIGHT, to = HNJOINT.HANDTIP_RIGHT, index = 11, isJointDistanceDependentSize = false, sizeFactor = new Vector3(0.6f, 0.8f, 1.0f), sizeFrom = 10, radius = 0.4f });//11
        Bones.Add(new HNSkeletonBone() { from = HNJOINT.WRIST_RIGHT, to = HNJOINT.THUMB_RIGHT, index = 12, isJointDistanceDependentSize = false, sizeFactor = new Vector3(0.6f, 0.2f, 1.0f), sizeFrom = 11, radius = 0.4f, useCapsuleCollider = false });//12

        // Left Arm
        Bones.Add(new HNSkeletonBone() { from = HNJOINT.SHOULDER_LEFT, to = HNJOINT.ELBOW_LEFT, index = 13, isJointDistanceDependentSize = true, sizeFactor = new Vector3(0.6f, 0.5f, 1.0f), sizeFrom = (int)HNJOINT.SHOULDER_CENTER, sizeTo = HNJOINT.SHOULDER_LEFT, radius = 0.4f });//13
        Bones.Add(new HNSkeletonBone() { from = HNJOINT.ELBOW_LEFT, to = HNJOINT.WRIST_LEFT, index = 14, isJointDistanceDependentSize = false, sizeFactor = new Vector3(0.6f, 0.8f, 1.0f), sizeFrom = 13, radius = 0.4f });//14
        Bones.Add(new HNSkeletonBone() { from = HNJOINT.WRIST_LEFT, to = HNJOINT.HAND_LEFT, index = 15, isJointDistanceDependentSize = false, sizeFactor = new Vector3(0.6f, 1.2f, 1.0f), sizeFrom = 14, radius = 0.4f });//15
        Bones.Add(new HNSkeletonBone() { from = HNJOINT.HAND_LEFT, to = HNJOINT.HANDTIP_LEFT, index = 16, isJointDistanceDependentSize = false, sizeFactor = new Vector3(0.6f, 0.8f, 1.0f), sizeFrom = 15, radius = 0.4f });//16
        Bones.Add(new HNSkeletonBone() { from = HNJOINT.WRIST_LEFT, to = HNJOINT.THUMB_LEFT, index = 17, isJointDistanceDependentSize = false, sizeFactor = new Vector3(0.6f, 0.2f, 1.0f), sizeFrom = 16, radius = 0.4f, useCapsuleCollider = false });//17

        // Right Leg
        Bones.Add(new HNSkeletonBone() { from = HNJOINT.HIP_RIGHT, to = HNJOINT.KNEE_RIGHT, index = 18, isJointDistanceDependentSize = true, sizeFactor = new Vector3(0.6f, 1.6f, 1.0f), sizeFrom = (int)HNJOINT.HIP_RIGHT, sizeTo = HNJOINT.HIP_CENTER, radius = 0.4f });//18
        Bones.Add(new HNSkeletonBone() { from = HNJOINT.KNEE_RIGHT, to = HNJOINT.ANKLE_RIGHT, index = 19, isJointDistanceDependentSize = false, sizeFactor = new Vector3(0.6f, 0.8f, 1.0f), sizeFrom = 18, radius = 0.4f });//19
        Bones.Add(new HNSkeletonBone() { from = HNJOINT.ANKLE_RIGHT, to = HNJOINT.FOOT_RIGHT, index = 20, isTerminalBone = true, isJointDistanceDependentSize = false, sizeFactor = new Vector3(0.6f, 1.1f, 1.5f), sizeFrom = 19, radius = 0.3f });//20

        // Left Leg
        Bones.Add(new HNSkeletonBone() { from = HNJOINT.HIP_LEFT, to = HNJOINT.KNEE_LEFT, index = 21, isJointDistanceDependentSize = true, sizeFactor = new Vector3(0.6f, 1.6f, 1.0f), sizeFrom = (int)HNJOINT.HIP_LEFT, sizeTo = HNJOINT.HIP_CENTER, radius = 0.4f });//21
        Bones.Add(new HNSkeletonBone() { from = HNJOINT.KNEE_LEFT, to = HNJOINT.ANKLE_LEFT, index = 22, isJointDistanceDependentSize = false, sizeFactor = new Vector3(0.6f, 0.8f, 1.0f), sizeFrom = 21, radius = 0.4f });//22
        Bones.Add(new HNSkeletonBone() { from = HNJOINT.ANKLE_LEFT, to = HNJOINT.FOOT_LEFT, index = 23, isTerminalBone = true, isJointDistanceDependentSize = false, sizeFactor = new Vector3(0.6f, 1.1f, 1.5f), sizeFrom = 22, radius = 0.3f });//23
    }

    private float boneLength(Vector3[] points, HNSkeletonBone bone)
    {
        return jointDistance(points, bone.from, bone.to);
    }

    private float jointDistance(Vector3[] points, HNJOINT joint1, HNJOINT joint2)
    {
        Vector3 fromVector = points[(int)(joint1)];
        Vector3 toVector = points[(int)(joint2)];
        return Vector3.Distance(fromVector, toVector);
    }

    private Vector3[] calculateBoneSizes(Vector3[] points)
    {
        Vector3[] boneSizes = new Vector3[Bones.Count];
        foreach (HNSkeletonBone bone in Bones)
        {
            boneLength(points, bone);
            float jointWidth;
            if (bone.isJointDistanceDependentSize)
            {
                jointWidth = bone.sizeFactor.y * jointDistance(points, (HNJOINT)(bone.sizeFrom), bone.sizeTo);
            }
            else
            {
                jointWidth = bone.sizeFactor.y * boneSizes[bone.sizeFrom].y;
            }
            boneSizes[bone.index] = new Vector3(bone.sizeFactor.x * jointWidth, jointWidth, boneLength(points, bone) * bone.sizeFactor.z);
        }
        return boneSizes;
    }

    public static List<HNSkeletonBone> getBones()
    {
        return Bones;
    }
}
