<%@ Page Language="C#" AutoEventWireup="True" MasterPageFile="~/Root.master" Inherits="SignInModule" Title="Sign In" Codebehind="SignIn.aspx.cs" %>

<asp:Content runat="server" ContentPlaceHolderID="Head">
    <link rel="stylesheet" type="text/css" href='<%# ResolveUrl("~/Content/SignInRegister.css") %>' />
    <script type="text/javascript" src='<%# ResolveUrl("~/Content/SignInRegister.js") %>'></script>
</asp:Content>

<asp:Content ID="Content" ContentPlaceHolderID="PageContent" runat="server">
    <style>
        .dxtc-stripContainer .dxtcLiteDisabled_Office365 .dxtc-link {
            font-weight: bold !important;
            color: blue !important;
        }

        .incorrect-pw{
            display: flex;
            justify-content: center;
            padding-top: 0.5rem;
        }
    </style>
    <div class="formLayout-verticalAlign">
        <div class="formLayout-container">
            <div class="disabled-text">
                <dx:ASPxTabControl ID="SignInRegisterTabControl" runat="server" Width="100%" TabAlign="Justify" Paddings-Padding="0">
                        <Tabs>
                            <dx:Tab Text="Sign in using Fusion Live" NavigateUrl="SignIn.aspx" Enabled="false">
                            </dx:Tab>
                        </Tabs>
                </dx:ASPxTabControl>
            </div>
            <asp:Label ID="ErrorMessageLabel" runat="server" ForeColor="Red" Visible="false" CssClass="incorrect-pw" />
            <dx:ASPxFormLayout runat="server" ID="FormLayout" ClientInstanceName="formLayout" UseDefaultPaddings="false">
                <SettingsAdaptivity AdaptivityMode="SingleColumnWindowLimit" />
                <SettingsItemCaptions Location="Top" />
                <Styles LayoutGroup-Cell-Paddings-Padding="0" LayoutItem-Paddings-PaddingBottom="8" />
                <Items>
                    <dx:LayoutGroup ShowCaption="False" GroupBoxDecoration="None" Paddings-Padding="16">
                        <Items>
                            <dx:LayoutItem Caption="Username" HelpText="Please, enter your Fusion Live Username">
                                <LayoutItemNestedControlCollection>
                                    <dx:LayoutItemNestedControlContainer>
                                        <dx:ASPxTextBox ID="SignInUserTextBox" runat="server" Width="100%">
                                            <ValidationSettings Display="Dynamic" SetFocusOnError="true" ErrorTextPosition="Bottom" ErrorDisplayMode="ImageWithText">
                                                <RequiredField IsRequired="true" ErrorText="Username is required" />
                                            </ValidationSettings>
                                            <ClientSideEvents Init="function(s, e){ s.Focus(); }" />
                                        </dx:ASPxTextBox>
                                    </dx:LayoutItemNestedControlContainer>
                                </LayoutItemNestedControlCollection>
                            </dx:LayoutItem>

                            <dx:LayoutItem Caption="Password">
                                <LayoutItemNestedControlCollection>
                                    <dx:LayoutItemNestedControlContainer>
                                        <dx:ASPxButtonEdit ID="PasswordButtonEdit" runat="server" Width="100%" Password="true" ClearButton-DisplayMode="Never">
                                            <ButtonStyle Border-BorderWidth="0" Width="6" CssClass="eye-button" HoverStyle-BackColor="Transparent" PressedStyle-BackColor="Transparent">
                                            </ButtonStyle>
                                            <ButtonTemplate>
                                                <div></div>
                                            </ButtonTemplate>
                                            <Buttons>
                                                <dx:EditButton>
                                                </dx:EditButton>
                                            </Buttons>
                                            <ValidationSettings Display="Dynamic" SetFocusOnError="true" ErrorTextPosition="Bottom" ErrorDisplayMode="ImageWithText">
                                                <RequiredField IsRequired="true" ErrorText="Password is required" />
                                            </ValidationSettings>
                                            <ClientSideEvents ButtonClick="onPasswordButtonEditButtonClick" />
                                        </dx:ASPxButtonEdit>
                                    </dx:LayoutItemNestedControlContainer>
                                </LayoutItemNestedControlCollection>
                            </dx:LayoutItem>

                            <dx:LayoutItem ShowCaption="False" Name="RememberMeAndLink" ColumnSpan="1">
                                <LayoutItemNestedControlCollection>
                                    <dx:LayoutItemNestedControlContainer>
                                        <table style="width: 100%">
                                            <tr>
                                                <td style="width: 65%">
                                                    <dx:ASPxCheckBox ID="RememberMeCheckBox" runat="server" Text="Remember me" Checked="true" Width="100%">
                                                    </dx:ASPxCheckBox>
                                                </td>
                                            </tr>
                                        </table>
                                    </dx:LayoutItemNestedControlContainer>
                                </LayoutItemNestedControlCollection>
                            </dx:LayoutItem>

                            <dx:LayoutItem ShowCaption="False" Name="GeneralError">
                                <LayoutItemNestedControlCollection>
                                    <dx:LayoutItemNestedControlContainer>
                                        <div id="GeneralErrorDiv" runat="server" class="formLayout-generalErrorText"></div>
                                    </dx:LayoutItemNestedControlContainer>
                                </LayoutItemNestedControlCollection>
                            </dx:LayoutItem>
                        </Items>
                    </dx:LayoutGroup>

                    <dx:LayoutGroup GroupBoxDecoration="HeadingLine" ShowCaption="False">
                        <Paddings PaddingTop="13" PaddingBottom="13" />
                        <GroupBoxStyle CssClass="formLayout-groupBox" />
                        <Items>

                            <dx:LayoutItem ShowCaption="False" HorizontalAlign="Center" Paddings-Padding="0">
                                <LayoutItemNestedControlCollection>
                                    <dx:LayoutItemNestedControlContainer>
                                        <dx:ASPxButton ID="SignInButton" runat="server" Width="200" Text="Sign In" OnClick="SignInButton_Click"></dx:ASPxButton>
                                    </dx:LayoutItemNestedControlContainer>
                                </LayoutItemNestedControlCollection>
                            </dx:LayoutItem>

                            <dx:LayoutItem ShowCaption="False" HorizontalAlign="Center" Paddings-Padding="10">
                                <LayoutItemNestedControlCollection>
                                    <dx:LayoutItemNestedControlContainer>
                                        <dx:ASPxLabel ID="loginLabel" runat="server" Text=""></dx:ASPxLabel>
                                    </dx:LayoutItemNestedControlContainer>
                                </LayoutItemNestedControlCollection>
                            </dx:LayoutItem>

                        </Items>
                    </dx:LayoutGroup>

                </Items>
            </dx:ASPxFormLayout>
        </div>
    </div>

</asp:Content>
