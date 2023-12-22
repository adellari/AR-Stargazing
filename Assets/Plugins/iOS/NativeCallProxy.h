#import <Foundation/Foundation.h>

typedef void (*TestDelegate)(const char* data, int length);
typedef void (*ProjectionDelegate)(int flag);
typedef void (*WavelengthDelegate)(int flag);
// NativeCallsProtocol defines protocol with methods you want to be called
// from managed.

// The communication via native calls is done using a delegate. Developer on the
// iOS side will register a delegate to Unity, and the `NativeCallProxy` file
// will be in charge of bridging Unity's call to the iOS delegate.
@protocol NativeCallsProtocol
@required
- (void) onUnityStateChange:(const NSString*) state;
- (void) onCompassUpdate:(Float32) compassVal;
- (void) onSetTestDelegate:(TestDelegate) delegate;
- (void) onSetProjectionDelegate:(ProjectionDelegate) delegate;
- (void) onSetWavelengthDelegate:(WavelengthDelegate) delegate;
// other methods
@end

__attribute__ ((visibility("default")))
@interface FrameworkLibAPI : NSObject
// call it any time after UnityFrameworkLoad to set object implementing NativeCallsProtocol methods
+(void) registerAPIforNativeCalls:(id<NativeCallsProtocol>) aApi;

@end
