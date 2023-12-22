#import <Foundation/Foundation.h>
#import "NativeCallProxy.h"

@implementation FrameworkLibAPI

id<NativeCallsProtocol> api = NULL;
+(void) registerAPIforNativeCalls:(id<NativeCallsProtocol>) aApi
{
    api = aApi;
}

@end

/**
 * The methods below bridge the calls from Unity into iOS. When Unity call any
 * of the methods below, the call is forwarded to the iOS bridge using the
 * `NativeCallsProtocol`.
 */
extern "C" {

void
sendUnityStateUpdate(const char* state)
{
    const NSString* str = @(state);
    [api onUnityStateChange: str];
}

void
sendCompassUpdate(Float32 compassVal)
{
    [api onCompassUpdate: compassVal];
}

void
setTestDelegate(TestDelegate delegate)
{
    TestDelegate newDelegate = delegate;
    [api onSetTestDelegate: newDelegate];
}

void
setProjectionDelegate(ProjectionDelegate delegate)
{
    ProjectionDelegate newDelegate = delegate;
    [api onSetProjectionDelegate: newDelegate];
}

void
setWavelengthDelegate(WavelengthDelegate delegate)
{
    WavelengthDelegate newDelegate = delegate;
    [api onSetWavelengthDelegate: newDelegate];
    
}

}
