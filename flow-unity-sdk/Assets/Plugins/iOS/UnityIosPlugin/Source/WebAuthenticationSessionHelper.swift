import UIKit
import AuthenticationServices

@objc public class WebAuthenticationSessionHelper: NSObject {
    @objc var session: ASWebAuthenticationSession?
    @objc public init(window: UIWindow, webUrl: String, appUrl: String, completion: ((URL?, Error?) -> Void)?){
        super.init()
        guard let requestWebUrl = URL(string: webUrl) else {
            completion?(nil, BloctoError.urlNotFound)
            return
        }

        guard let requestAppUrl = URL(string: appUrl) else {
            completion?(nil, BloctoError.urlNotFound)
            return
        }

        let urlOpening: URLOpening = UIApplication.shared
        urlOpening.open(requestAppUrl, options: [.universalLinksOnly: true], completionHandler: { (success) in
            if success {
                self.log(enable: true, message: "url scheme should be rather than \(String(describing: requestAppUrl.scheme)).")
                return
            } else {
                
                self.log(enable: true, message: "can't open universal link")
                session = ASWebAuthenticationSession(
                    url: requestWebUrl,
                    callbackURLScheme: "blocto",
                    completionHandler: { callbackURL, error in
                        completion?(callbackURL, error)
                        session = nil
                    })
                
                if #available(iOS 13.0, *) {
                    session?.presentationContextProvider = window
                }
                
                let startsSuccessfully = session?.start()
                if startsSuccessfully == false {
                    // handle error
                    completion?(nil, BloctoError.startFailed)
                }
            }
        })
    }

    func log(enable: Bool, message: String) {
        guard enable else { return }
        print("BloctoSDK: " + message)
    }
}

extension UIWindow: ASWebAuthenticationPresentationContextProviding {

    public func presentationAnchor(for session: ASWebAuthenticationSession) -> ASPresentationAnchor {
        return self
    }

}

enum BloctoError: Swift.Error {
    case urlNotFound
    case startFailed
}
