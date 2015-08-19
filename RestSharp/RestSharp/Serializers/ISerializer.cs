#region License
//   Copyright 2010 John Sheehan
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License. 
#endregion

namespace RestSharp.Serializers
{
    public interface ISerializer
    {
        /// <summary>
        /// Defines which object to Serialize.
        /// </summary>
        /// <param name="obj">Object to serialize.</param>
        /// <returns></returns>
        string Serialize(object obj);

       /// <summary>
        /// Gets or sets the RootElement.
       /// </summary>
       string RootElement { get; set; }

        /// <summary>
        /// Gets or sets the Namespace
        /// </summary>
        string Namespace { get; set; }

        /// <summary>
        /// Gets or sets the DateFormat
        /// </summary>
        string DateFormat { get; set; }
        
        /// <summary>
        /// Gets or sets the ContentType
        /// </summary>
        string ContentType { get; set; }
    }
}
