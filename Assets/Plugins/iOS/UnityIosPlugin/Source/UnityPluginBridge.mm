#import <Foundation/Foundation.h>
#import <AuthenticationServices/AuthenticationServices.h>
#include "UnityFramework/UnityFramework-Swift.h"

#pragma marc - C interface

extern "C" {

#pragma mark - Functions
     WebAuthenticationSessionHelper *sessionHelper = [WebAuthenticationSessionHelper sharedInstance];
     void OpenUrl(const char* goName, const char* callFnName, const char* webUrl, const char* appUrl){
          NSString* go = [NSString stringWithUTF8String:goName]; // c 字符串 转成 oc 字符串, 这里一定要先转成 oc, 不然 const char* 调用后就会释放掉栈内存, 会导致 UnitySendMessage 回传 unity 失败
          NSString* fn = [NSString stringWithUTF8String:callFnName];
          NSString* failedFn = [NSString stringWithUTF8String:"FailedHandler"];
          NSString* tmpWebUrl = [NSString stringWithUTF8String:webUrl];
          NSString* tmpAppUrl = [NSString stringWithUTF8String:appUrl];
         
          NSLog(@"%@", failedFn);
          
          UIWindow *keyWindow = nil;
          
          NSSet *connectedScenes = [UIApplication sharedApplication].connectedScenes;
          for (UIScene *scene in connectedScenes) {
               if ([scene isKindOfClass:[UIWindowScene class]]) {
                    UIWindowScene *windowScene = (UIWindowScene *)scene;
                    for (UIWindow *window in windowScene.windows) {
                         if (window.isKeyWindow) {
                              keyWindow = window;
                              break;
                         }
                    }
               }
          }
          
          if (keyWindow) {
               [sessionHelper openWebViewWithWindow:keyWindow webUrl:tmpWebUrl appUrl:tmpAppUrl completion:^(NSURL * _Nullable url, NSError * _Nullable error) {
                   if (error) {
                        // handle error here.
                        NSLog(@"In Error.");
                        UnitySendMessage([go UTF8String], [failedFn UTF8String], "Swift failed.");
                   }
                   if (url) {
                        // handle url here.
                        NSLog(@"In Success.");
                        
                        const char* deeplink = [[NSString stringWithFormat: @"%@", url] UTF8String]; // oc 字符串 转成 c 字符串
                        UnitySendMessage([go UTF8String], [fn UTF8String], deeplink); // ios 调用 unity
                   }
               }];
          } else {
               // handle key window not found.
          }
     }

     void CloseWindow()
     {
         NSLog(@"In objc Close Windows");
         return [sessionHelper closeWindow];
     }
}

char* cStringCopy(const char* string){
     if (string == NULL){
          return NULL;
     }
     char* res = (char*)malloc(strlen(string)+1);
     strcpy(res, string);
     return res;
}
