### 0.4.4 - Lower newtonsoft version requirement
* Newtonsoft 7 -> 6

### 0.4.3 - Remove logging level concept
* Remove the idea of a logging level and make it specific to the implementation

### 0.4.2 - Add Excution onSuccess
* Add onSuccess function to execution

### 0.4.1 - Minor update to logging
* prefixName -> prefixGroupName, and now puts a dot between prefix and the name

### 0.4 - Moved Api library
* Api library has been moved to Trustpilot.FSharp.Flow.Api

### 0.3 - Introducing RequestHandler
* Added a request handler for Api library

### 0.2.2 - Changed System.Net.Http dependency
* Set System.Net.Http dependency to >= 2.0.20710

### 0.2.1 - Set System.Net.Http reference
* Set System.Net.Http reference to be >= 4

### 0.2 - Simplification and Restructuring
* Moved result and flow into the Trustpilot.FSharp namespace
* Added Flow.catchMap
* Added fromApiFlow and fromStoreFlow to AppFlow

### 0.1 - Release of Flow package
* Added Flow