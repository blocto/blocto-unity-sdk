#import <Foundation/Foundation.h>

@interface UniversalLink : NSObject

@property(nonatomic, strong) NSString *URL;

+ (UniversalLink *)instance;

- (void)reset;

-(NSString *)getUrl;

@end
