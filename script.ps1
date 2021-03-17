        # Write your PowerShell commands here.
        $url = "https://wvd-scale.azurewebsites.net/api/DeplymentStartProcess?code=$(functionKey2)"
        $Body = @{sessionId = "${{ parameters.sessionId }}"}
        $Parameters = @{
          Method = "POST"
          Uri =  $url
          Body = ($Body | ConvertTo-Json) 
          ContentType = "application/json"
        }
        Invoke-RestMethod @Parameters
        # Set response to a variable
        # https://gaunacode.com/passing-variables-between-jobs-for-azure-devops-pipelines