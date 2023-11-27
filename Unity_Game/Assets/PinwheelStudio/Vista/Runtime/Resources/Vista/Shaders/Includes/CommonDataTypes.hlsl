#ifndef COMMON_DATA_TYPE_INCLUDED
#define COMMON_DATA_TYPE_INCLUDED

const int TREE_SAMPLE_SIZE = 7;
struct InstanceSample
{
	float isValid;
	float3 position;
	float verticalScale; 
	float horizontalScale; 
	float rotationY; 
};

const int POSITION_SAMPLE_SIZE = 4;
struct PositionSample
{
	float isValid;
	float3 position;
};

#endif // COMMON_DATA_TYPE_INCLUDED