import UIKit
import AuthenticationServices

@objc public class WebAuthenticationSessionHelper: NSObject {
    @objc var session: ASWebAuthenticationSession?
    @objc public class var sharedInstance : WebAuthenticationSessionHelper {
        struct Static {
            static let instance : WebAuthenticationSessionHelper = WebAuthenticationSessionHelper()
        }

        return Static.instance
    }

    @objc public func isInstallation(urlString:String) -> Bool {
        log(enable: true, message: "In swift isInstallation")
        var isInstalled = false
        if let appURL = URL(string: urlString) {
            isInstalled = UIApplication.shared.canOpenURL(appURL)
            print("Can open \"\(appURL)\": \(isInstalled)")
            return isInstalled
        }
        
        print("Can open blocto-staging://open: \(isInstalled)")
        return isInstalled
    }

    @objc public func openUrl(window: UIWindow, webUrl: String, appUrl: String, completion: ((URL?, Error?) -> Void)?){
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
                self.session = ASWebAuthenticationSession(
                    url: requestWebUrl,
                    callbackURLScheme: "blocto",
                    completionHandler: { callbackURL, error in
                        completion?(callbackURL, error)
                        self.session = nil
                    })
                
                if #available(iOS 13.0, *) {
                    self.session?.presentationContextProvider = window
                }
                
                let startsSuccessfully = self.session?.start()
                if startsSuccessfully == false {
                    // handle error
                    self.log(enable: true, message: "star is failed.")
                    completion?(nil, BloctoError.startFailed)
                }
            }
        })
    }

    @objc public func openWebView(window: UIWindow, webUrl: String, appUrl: String, completion: ((URL?, Error?) -> Void)?){
        guard let requestWebUrl = URL(string: webUrl) else {
            completion?(nil, BloctoError.urlNotFound)
            return
        }

        guard let requestAppUrl = URL(string: appUrl) else {
            completion?(nil, BloctoError.urlNotFound)
            return
        }

        self.session = ASWebAuthenticationSession(
                    url: requestWebUrl,
                    callbackURLScheme: "blocto",
                    completionHandler: { callbackURL, error in
                        completion?(callbackURL, error)
                        self.session = nil
                    })
                
        if #available(iOS 13.0, *) {
            self.session?.presentationContextProvider = window
        }
        
        let startsSuccessfully = self.session?.start()
        if startsSuccessfully == false {
            // handle error
            self.log(enable: true, message: "star is failed.")
            completion?(nil, BloctoError.startFailed)
        }
    }

    @objc public func closeWindow()
    {
        log(enable: true, message: "In swift closeWindow")
        self.session?.cancel()
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
