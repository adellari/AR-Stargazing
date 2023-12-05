// Serialized structure sent to Unity.
///
/// This is used on the Unity side to decide what to do when a message
/// arrives.
struct MessageWithData<T: Encodable>: Encodable {
    var type: String
    var data: T
}

/// Swift API to handle Native <> Unity communication.
///
/// - Note:
///   - Message passing is done via serialized JSON
///   - Message passing is done via function pointer exchanged between Unity <> Native
public class UnityAPI: NSObject, NativeCallsProtocol {
    public var testDelegate: TestDelegate!
    
    public func onSetTestDelegate(_ delegate: TestDelegate!) {
        self.testDelegate = delegate
    }
    
    
    
    
  
    

    // Name of the gameobject that receives the
    // messages from the native side.
    private static let API_GAMEOBJECT = "APIEntryPoint"
    // Name of the method to call when sending
    // messages from the native side.
    private static let API_MESSAGE_FUNCTION = "ReceiveMessage"

    public weak var communicator: UnityCommunicationProtocol!
    public var ready: () -> () = {}

    /**
        Function pointers to static functions declared in Unity
     */



    public override init() {}


    
    
    /**
     * Public API for developers.
     */

    /// Friendly wrapper arround the message passing system.
    ///
    /// - Note:
    /// This wrapper is used to get friendlier API for Swift developers.
    /// They shouldn't have to care about how the color is sent to Unity.
    public func setColor(r: CGFloat, g: CGFloat, b: CGFloat) {
        let data = [r, g, b]
        sendMessage(type: "change-color", data: data)
    }
    
    public func armColor(val: Int){
        sendMessage(type: "arm-color", data: val)
    }

    public func armAnimation(val: String){
        sendMessage(type: "arm-animation", data: val)
    }

    public func armSet(val: Int){
        sendMessage(type: "set-arm", data: val)
    }
    
    public func jointRotation(orientation: [UInt8]){
        sendMessage(type: "jointR", data: orientation)
    }

    public func jointPosition(position: [UInt8]){
        sendMessage(type: "jointP", data: position)
    }
    
    public func test(_ value: Data) {
        value.withUnsafeBytes { bytes in
            let charPointer = bytes.baseAddress?.assumingMemoryBound(to: CChar.self)
            self.testDelegate?(charPointer, Int32(value.count))
        }
    }

    /**
     * Internal API.
     */

    @objc public func onUnityStateChange(_ state: String) {
        switch (state) {
        case "ready":
            self.ready()
        default:
            return
        }
    }
    

    /**
     * Private  API.
     */

    private func sendMessage<T: Encodable>(type: String, data: T) {
            let message = MessageWithData(type: type, data: data)
            let encoder = JSONEncoder()
            let json = try! encoder.encode(message)
            communicator.sendMessageToGameObject(
                go: UnityAPI.API_GAMEOBJECT,
                function: UnityAPI.API_MESSAGE_FUNCTION,
                message: String(data: json, encoding: .utf8)!
            )
        }
    
    /// Internal function sending message to Unity.
    private func sendBinaryMessage<T: Encodable>(type: String, data: T) {
        let message = MessageWithData(type: type, data: data)
        let encoder = JSONEncoder()
        if let jsonData = try? encoder.encode(message) {
            let jsonBase64String = jsonData.base64EncodedString()
            communicator.sendMessageToGameObject(
                go: UnityAPI.API_GAMEOBJECT,
                function: UnityAPI.API_MESSAGE_FUNCTION,
                message: jsonBase64String
            )
        } else {
            print("Failed to encode message")
        }
    }
}
