//{
//  "domain": "https://dev-00450157.okta.com",
//    "apiToken": "00egZkhqJ2edpRWydEvxQZb1JqYP5HdOjLGsYL7qI_",
//      "lastSync": "2022-08-13T21:35:58Z",
//        "groupsMode": 2,
//          "groups": null,
//            "usersMode": 2,
//              "users": null
//}

import { observer } from "mobx-react-lite";
import React from "react";

/** React component for user introduction to the feature */
export const OktaIntro: React.FC = observer(() => {
  return (
    <div className="mb-3">
      <p>
        <a href="https://www.okta.com/" target="_blank" rel="noreferrer">
          Okta
        </a>{" "}
        is an identity provided used by companies around the world to provide
        SSO across business apps. Soundbite is able to connect to your{" "}
        <a
          href="https://www.okta.com/products/universal-directory/"
          target="_blank"
          rel="noreferrer"
        >
          Okta directory
        </a>{" "}
        and user users by their e-mail. When they login, they'll be able to
        access Soundbite with one click.
      </p>
    </div>
  );
});
