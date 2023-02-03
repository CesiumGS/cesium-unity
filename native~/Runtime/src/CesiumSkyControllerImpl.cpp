#include "CesiumSkyControllerImpl.h"

#include <CesiumGeospatial/SunPosition.h>

#include <DotNet/CesiumForUnity/CesiumSkyController.h>
#include "UnityTransforms.h"

#include <glm/vec3.hpp>

#include <DotNet/UnityEngine/Vector3.h>

using namespace CesiumGeospatial;
using namespace DotNet;

namespace CesiumForUnityNative {

	CesiumSkyControllerImpl::CesiumSkyControllerImpl(
    const DotNet::CesiumForUnity::CesiumSkyController& skyController) {}

	CesiumSkyControllerImpl::~CesiumSkyControllerImpl() {}

	void CesiumSkyControllerImpl::JustBeforeDelete(
            const DotNet::CesiumForUnity::CesiumSkyController& skyController) {}

	UnityEngine::Vector3
        CesiumSkyControllerImpl::CalculateSunPosition(
            const DotNet::CesiumForUnity::CesiumSkyController&
                skyController, float& time) {

		glm::dvec3 cppAngle = SunPosition::getSunAngle(time);

		UnityEngine::Vector3 sunAngle;
		sunAngle = UnityTransforms::toUnity(cppAngle);

		return sunAngle;
	}
 }
